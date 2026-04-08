using Unity.Collections;
using Unity.Entities;
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
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);

        var loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;


        foreach ((EnabledRefRW<WaitForProcess> wait,RefRO<ServerEventData> eventData) in
        SystemAPI.Query<EnabledRefRW<WaitForProcess>,RefRO<ServerEventData>>())
        {
            if(!tick.IsNewerThan(eventData.ValueRO.tick))
            {
                wait.ValueRW = false;
            }
        }

        
        foreach ((RefRO<PlayerActionRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<PlayerActionRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;  
            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                RPCHelper.CreateSerwerLocalEvent(new FutureReloadRPC(),ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(tick,item.cooldown));
            }

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }
    
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<FutureReloadRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<FutureReloadRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection; 
            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
                ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
                if(ammo.Length > 0)
                {
                    var playerInputSync = state.EntityManager.GetComponentData<PlayerInputSync>(player);
                    var playerInput = state.EntityManager.GetComponentData<PlayerInput>(player);
                    var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);

                    var selectedAmmo = playerInput.ammoSelectedIndex % ammo.Length;
                    var ammoID = ammo[selectedAmmo].itemId;
                    playerInputSync.ammoSelectedItemID = ammoID;
                    playerInputSync.ammoSelectedIndex = selectedAmmo;
                    
                    RPCHelper.SendEventsToClientsAndOwner<NewAmmoSelectedRPC>(new NewAmmoSelectedRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,
                    ref state,playerNeedChunkLookup,loadedChunks,ecb,rpc.ValueRO.networkID,player,ghostChunk.GetChunk(),rpc.ValueRO.tick,true);          
                    state.EntityManager.SetComponentData(player,playerInputSync);
                }
            }
            ecb.DestroyEntity(entity);
        }
         
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<NewItemInHandRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<NewItemInHandRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,20)});

            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
            ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
            {
                var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
                if(ammo.Length > 0)
                {
                    var playerInputSync = state.EntityManager.GetComponentData<PlayerInputSync>(player);
                    var playerInput = state.EntityManager.GetComponentData<PlayerInput>(player);
                    var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);

                    var selectedAmmo = playerInput.ammoSelectedIndex % ammo.Length;
                    var ammoID = ammo[selectedAmmo].itemId;
                    playerInputSync.ammoSelectedItemID = ammoID;
                    playerInputSync.ammoSelectedIndex = selectedAmmo;
                    
                    RPCHelper.SendEventsToClientsAndOwner<NewAmmoSelectedRPC>(new NewAmmoSelectedRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,
                    ref state,playerNeedChunkLookup,loadedChunks,ecb,rpc.ValueRO.networkID,player,ghostChunk.GetChunk(),rpc.ValueRO.tick,true);
                    
                    state.EntityManager.SetComponentData(player,playerInputSync);
                }
                Debug.Log("nowe kkkk!!! " + tick.TickIndexForValidTick);
            }

            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }
  
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<NewAmmoSelectedRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<NewAmmoSelectedRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            if(ItemsAsset.instance.TryGetItem<RangedWeapon>(rpc.ValueRO.weaponID,out var item))
                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpc.ValueRO.tick,item.reloadCooldown)});
            
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);               
            ecb.DestroyEntity(entity);
        }


        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


}
