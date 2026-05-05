using System;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct RPCProcessingSystem : ISystem
{

    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;
    private BufferLookup<LinkedContainers> linkedContainers;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<LoadedChunks>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<SendEventToPlayers>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
        linkedContainers = SystemAPI.GetBufferLookup<LinkedContainers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);
        linkedContainers.Update(ref state);

        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        var loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;


        foreach ((EnabledRefRW<WaitForProcess> wait,RefRO<ServerEventData> eventData) in
        SystemAPI.Query<EnabledRefRW<WaitForProcess>,RefRO<ServerEventData>>())
        {
            if(!eventData.ValueRO.tick.IsNewerThan(tick))
            {
                wait.ValueRW = false;
            }          
        }
 


        foreach ((RefRO<PlayerActionRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<PlayerActionRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;  
            state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state = PlayerState.shooting});
            
            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item) && !item.hasMagazine)
            {
                RPCHelper.CreateSerwerLocalEvent(new FutureReload(),ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(tick,item.shootCooldown),false);
            }

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }

        foreach ((RefRO<FutureReload> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<FutureReload>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection; 
            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
                var playerInputSync = state.EntityManager.GetComponentData<PlayerInputSync>(player);

                if(ammo.Length > 0)
                {
                    var playerInput = state.EntityManager.GetComponentData<PlayerInput>(player);
                    var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);
                    var ammoID = playerInputSync.ammoSelectedItemID;

                    if(!EQHelper.PlayerHasTheAmmo(playerInputSync.ammoSelectedItemID,ammo))
                    {
                        var selectedAmmo = playerInput.ammoSelectedIndex % ammo.Length;
                        ammoID = ammo[selectedAmmo].itemId;
                        playerInputSync.ammoSelectedItemID = ammoID;
                        playerInputSync.ammoSelectedIndex = selectedAmmo;
                    }
                    
                    RPCHelper.SendEventsToClientsAndOwner<NewAmmoSelectedRPC>(new NewAmmoSelectedRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,
                    ref state,playerNeedChunkLookup,loadedChunks,ecb,rpc.ValueRO.networkID,player,ghostChunk.GetChunk(),rpc.ValueRO.tick,true);          
                }
                else
                {
                    playerInputSync.ammoSelectedItemID = -1;
                }
                state.EntityManager.SetComponentData(player,playerInputSync);
            }
            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            CleanUpEventBuffer(buffer,entity);

            ecb.DestroyEntity(entity);
        }
         
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        containersLookup.Update(ref state);   
        slotsLookup.Update(ref state);    
         


        foreach ((RefRO<NewItemInHandRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<NewItemInHandRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,20)});
            var playerInputSync = state.EntityManager.GetComponentData<PlayerInputSync>(player);

            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,playerInputSync.slotInHand), out var slot,out int bufferIndex) && 
            ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                if(!item.hasMagazine)
                {
                    var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
                    if(ammo.Length > 0)
                    {                 
                        var playerInput = state.EntityManager.GetComponentData<PlayerInput>(player);
                        var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);   
                        var selectedAmmo = playerInput.ammoSelectedIndex % ammo.Length;
                        var ammoID = ammo[selectedAmmo].itemId;

                        playerInputSync.ammoSelectedItemID = ammoID;
                        playerInputSync.ammoSelectedIndex = selectedAmmo;
                        playerInputSync.ammoSelectedTagID = item.ammoTagID;

                        RPCHelper.SendEventsToClientsAndOwner<NewAmmoSelectedRPC>(new NewAmmoSelectedRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,
                        ref state,playerNeedChunkLookup,loadedChunks,ecb,rpc.ValueRO.networkID,player,ghostChunk.GetChunk(),rpc.ValueRO.tick,true);                    
                    }
                    else
                    {
                        playerInputSync.ammoSelectedTagID = item.ammoTagID;
                        playerInputSync.ammoSelectedItemID = -1;
                    } 
                }
                else
                {
                    playerInputSync.ammoSelectedTagID = item.ammoTagID;
                    playerInputSync.ammoSelectedItemID = -1;
                }            
            }
            else
            {
                playerInputSync.ammoSelectedTagID = -1;
                playerInputSync.ammoSelectedItemID = -1;
            }

            state.EntityManager.SetComponentData(player,playerInputSync);
            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            StopFutureEvents(ref state,ecb,buffer,typeof(FutureReload),typeof(EndReload),typeof(EndUnload));

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }
  
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);


        foreach ((RefRO<ReloadRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<ReloadRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            StopFutureEvents(ref state,ecb,buffer,typeof(FutureReload),typeof(EndReload),typeof(EndUnload));

            if(ItemsAsset.instance.TryGetItem<RangedWeapon>(rpc.ValueRO.weaponID,out var item))
            {
                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,item.reloadCooldown)});               
                state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state = item.reloadingState});
                RPCHelper.CreateSerwerLocalEvent(new EndReload(){ammoID = rpc.ValueRO.ammoID},ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(tick,item.reloadCooldown),false);
            }
            
            var newRPC = rpc.ValueRO.GetRPC();
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in newRPC);               
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<EmptyMagazineRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<EmptyMagazineRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            
            Debug.Log("wysylam!");
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
                
                
        foreach ((RefRO<UnloadRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<UnloadRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            StopFutureEvents(ref state,ecb,buffer,typeof(FutureReload),typeof(EndReload),typeof(EndUnload));

            
            if(ItemsAsset.instance.TryGetItem<RangedWeapon>(rpc.ValueRO.weaponID,out var item))
            {
                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,item.unloadCooldown)});
                state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state =  PlayerState.unloading});
                RPCHelper.CreateSerwerLocalEvent(new EndUnload(),ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(tick,item.unloadCooldown),false);
            }


            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }

        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<EndUnload> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRO<EndUnload>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            var input = SystemAPI.GetComponent<PlayerInputSync>(player);


            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,input.slotInHand), out var slot,out int bufferIndex,out Entity containerEntity) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                var ammo = EQHelper.TryFindItemWithTag(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out var aggregated,out int counter);
                var items = EQHelper.ReadLinkedContainer(slotsLookup,linkedContainers,containerEntity,input.slotInHand, out Entity linkedContainerEntity);
                if(items != null && items.Length > 0)
                {
                    EQHelper.SubtractItem(slotsLookup,linkedContainerEntity,0,out var removedValue,1,false);  
                    var slots = EQHelper.FindSlotForItem(ref state, slotsLookup, containersLookup, player,removedValue.Value);
                    if(slots.Length > 0)
                    {
                        var events = EQHelper.AddItems(ref state,ecb,ref entitiesReferences,linkedContainers,barsLookup,slotsLookup,containersLookup, player, slots,new EQGiveItem(removedValue.Value));    
                        EQHelper.SendEvents(ref ecb, rpc.ValueRO.networkID, events);
                        EQHelper.SendEvents(ecb, rpc.ValueRO.networkID,new EquipmentEvent(EquipementEventFlags.UpdateWeaponMagazine));        
                        if(items.Length > 1 && EQHelper.FindSlotForItem(ref state,slotsLookup,containersLookup,player,items[1].itemId).Length > 0)
                        {
                            var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);
                            RPCHelper.SendEventsToClientsAndOwner<UnloadRPC>(new UnloadRPC(){ weaponID =  slot.Value.itemId, ammoID = items[1].itemId} ,ref state,playerNeedChunkLookup,loadedChunks,ecb,rpc.ValueRO.networkID,player,ghostChunk.GetChunk(),tick);
                        }     
                    }                     
                }
            }

            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            CleanUpEventBuffer(buffer,e);
            ecb.DestroyEntity(e);
        }
       

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<EndReload> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRO<EndReload>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            var input = SystemAPI.GetComponent<PlayerInputSync>(player);

            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,input.slotInHand), out var slot,out int bufferIndex,out Entity containerEntity) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item) && item.hasMagazine)
            {
                var ammo = EQHelper.TryFindItemWithTag(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out var aggregated,out int counter);
                int magazineCount = EQHelper.CountItemsInLinkedContainer(slotsLookup,linkedContainers,containerEntity,input.slotInHand, out Entity linkedContainerEntity);

                if(EQHelper.PlayerHasTheAmmo(rpc.ValueRO.ammoID,ammo,out int index))
                {
                    var events = EQHelper.SubtractItem(slotsLookup,containersLookup,ammo[index].transferData.pos,player,out var template);
                    EQHelper.SendEvents(ecb, rpc.ValueRO.networkID, events);
                    EQHelper.AppendToBuffer(slotsLookup,linkedContainerEntity,template.Value);
                    if(magazineCount + 1 < item.magazineCapacity && counter > 1)
                    {
                        var playerInput = state.EntityManager.GetComponentData<PlayerInput>(player);
                        var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);
                        int ammoID = aggregated[input.ammoSelectedIndex].itemId;
                        if(aggregated[input.ammoSelectedIndex].quantity == 1)
                            ammoID = aggregated[(input.ammoSelectedIndex + 1) % (aggregated.Length - 1)].itemId;
                        
                        RPCHelper.SendEventsToClientsAndOwner<ReloadRPC>(new ReloadRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,ref state,playerNeedChunkLookup,loadedChunks,ecb,rpc.ValueRO.networkID,player,ghostChunk.GetChunk(),tick);

                       // state.EntityManager.SetComponentData(player,input);  
                    }   
                }
            }

            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            CleanUpEventBuffer(buffer,e);
            ecb.DestroyEntity(e);
        }


        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        containersLookup.Update(ref state);   
        slotsLookup.Update(ref state);    

        foreach ((RefRW<StopReloadRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRW<StopReloadRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Debug.Log("reload koniec!");
            Entity player = toPlayers.ElementAt(0).connection;
            state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,20) });
            state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state =  PlayerState.none});
            EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex);
            rpc.ValueRW.weaponID = slot.HasValue ? slot.Value.itemId : -1;
   
            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            StopFutureEvents(ref state,ecb,buffer,typeof(FutureReload),typeof(EndReload),typeof(EndUnload));
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);   
            ecb.DestroyEntity(e);
        }


        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);


        foreach ((RefRO<NewAmmoSelectedRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<NewAmmoSelectedRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            if(ItemsAsset.instance.TryGetItem<RangedWeapon>(rpc.ValueRO.weaponID,out var item))
            {
                state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state = item.reloadingState});
                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,item.reloadCooldown)});
            }            

            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            StopFutureEvents(ref state,ecb,buffer,typeof(FutureReload),typeof(EndReload),typeof(EndUnload));

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }


        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


    private void StopFutureEvents(ref SystemState state,EntityCommandBuffer ecb,DynamicBuffer<FutureEventsForPlayer> buffer,params ComponentType[] components)
    {   
        for(int i = buffer.Length - 1; i >= 0;i--)
        {
            var eventEntity = buffer[i].entityEvent;
            foreach( var comp in components)
            {
                if(state.EntityManager.HasComponent(eventEntity,comp))
                {
                    buffer.RemoveAt(i);
                    ecb.DestroyEntity(eventEntity);
                }
            }
        }
    }


    private void CleanUpEventBuffer(DynamicBuffer<FutureEventsForPlayer> buffer, Entity entityEvent)
    {
        for(int i = buffer.Length - 1; i >= 0;i--)
        {
            if(buffer[i].entityEvent == entityEvent)
            {
                buffer.RemoveAtSwapBack(i);
                break;
            }
        }
    }

}
