using System;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
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
            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item) && !item.hasMagazine)
            {
                RPCHelper.CreateSerwerLocalEvent(new FutureReloadRPC(),ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(tick,item.cooldown),false);
            }

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }

        foreach ((RefRO<FutureReloadRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<FutureReloadRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
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

            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
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
            for(int i = buffer.Length - 1; i >= 0;i--)
            {
                var eventEntity = buffer[i].entityEvent;
                if(SystemAPI.HasComponent<FutureReloadRPC>(eventEntity))
                {
                    buffer.RemoveAt(i);
                    ecb.DestroyEntity(eventEntity);
                }
            }

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
            if(ItemsAsset.instance.TryGetItem<RangedWeapon>(rpc.ValueRO.weaponID,out var item))
            {
                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,item.reloadCooldown)});
                RPCHelper.CreateSerwerLocalEvent(new EndReloadRPC(){ammoID = rpc.ValueRO.ammoID},ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(tick,item.reloadCooldown),false);
            }

            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            for(int i = buffer.Length - 1; i >= 0;i--)
            {
                var eventEntity = buffer[i].entityEvent;
                if(SystemAPI.HasComponent<FutureReloadRPC>(eventEntity))
                {
                    buffer.RemoveAt(i);
                    ecb.DestroyEntity(eventEntity);
                }
            }
            var newRPC = rpc.ValueRO.GetRPC();

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in newRPC);               
            ecb.DestroyEntity(entity);
        }


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


        foreach ((RefRO<EndReloadRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRO<EndReloadRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Debug.Log("reload koniec!");

            Entity player = toPlayers.ElementAt(0).connection;
            var input = SystemAPI.GetComponent<PlayerInputSync>(player);
            

            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,input.slotInHand), out var slot,out int bufferIndex,out Entity containerEntity) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                var ammo = EQHelper.TryFindItemWithTag(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out var aggregated,out int counter);
                int magazineCount = EQHelper.CountItemsInLinkedContainer(slotsLookup,linkedContainers,containerEntity,input.slotInHand, out Entity linkedContainerEntity);

                if(EQHelper.PlayerHasTheAmmo(rpc.ValueRO.ammoID,ammo,out int index))
                {
                    var events = EQHelper.SubtractItem(slotsLookup,containersLookup,ammo[index].transferData.pos,player,out var template);
                    EQHelper.SendEvents(ecb, rpc.ValueRO.networkID, events);
                    Debug.Log("reload KKKKKKKKKKKKKKK " + linkedContainerEntity + " " + template.Value);
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

        foreach ((RefRO<NewAmmoSelectedRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<NewAmmoSelectedRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            if(ItemsAsset.instance.TryGetItem<RangedWeapon>(rpc.ValueRO.weaponID,out var item))
            {
                if(item.hasMagazine)
                    return;
                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,item.reloadCooldown)});
            }            

            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            for(int i = buffer.Length - 1; i >= 0;i--)
            {
                var eventEntity = buffer[i].entityEvent;
                if(SystemAPI.HasComponent<FutureReloadRPC>(eventEntity))
                {
                    buffer.RemoveAt(i);
                    ecb.DestroyEntity(eventEntity);
                }
            }

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }



        ecb.Playback(state.EntityManager);
        ecb.Dispose();
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
