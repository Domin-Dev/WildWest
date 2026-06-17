using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(RPCProcessingSystem))]
partial struct WorldItemServerSystem : ISystem
{

    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<EntityContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;
    private BufferLookup<LinkedContainers> linkedContainers;
    private ComponentLookup<ContainerComponent> componentLookup;
    private BufferLookup<WorldItemEntity> worldItemEntityLookup;
    private BufferLookup<WorldItemPosition> worldItemPositionLookup;

    private ComponentLookup<WorldItem> worldItemComponentLookup;


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<LoadedChunks>();
        state.RequireForUpdate<Chunks>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        containersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
        linkedContainers = SystemAPI.GetBufferLookup<LinkedContainers>();
        componentLookup = SystemAPI.GetComponentLookup<ContainerComponent>();
        worldItemEntityLookup = SystemAPI.GetBufferLookup<WorldItemEntity>();
        worldItemComponentLookup = SystemAPI.GetComponentLookup<WorldItem>();
        worldItemPositionLookup = SystemAPI.GetBufferLookup<WorldItemPosition>();

    }
    public void OnUpdate(ref SystemState state)
    {
        var chunks = SystemAPI.GetSingleton<Chunks>();
        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);
        linkedContainers.Update(ref state);
        componentLookup.Update(ref state);
        worldItemEntityLookup.Update(ref state);
        worldItemComponentLookup.Update(ref state);
        worldItemPositionLookup.Update(ref state);

        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRW<DropItemRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRW<DropItemRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;

            if(chunks.currentChunks.TryGetValue(rpc.ValueRO.chunkIndex,out Entity chunkEntity))
            { 
                if(EQHelper.TryGetPlayerContainer(containersLookup,chunkEntity,EquipmentConfig.chunkItems_ContainerIndex,out var container))
                {
                    if(EQHelper.TryGetBufferIndex(slotsLookup,rpc.ValueRO.slotIndex,container.Value.entity,out InventorySlot? item,out int bufferIndex))
                    {
                        RPCHelper.CreateLocalEvent(new SpawnWorldItem(){ 
                            chunkIndex = rpc.ValueRO.chunkIndex,
                            slotIndex = rpc.ValueRO.slotIndex,
                            dropPosition = rpc.ValueRO.dropPosition,
                            item = item.Value,
                            chunk = chunkEntity    
                        },ecb,EntityHelper.AddTime(rpc.ValueRO.tick,rpc.ValueRO.duration),false);

                        for(int i = 1; i < toPlayers.Length;i++)
                            RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);
                    }
                }
            }
            ecb.DestroyEntity(e);
        }
             
        foreach ((RefRW<PickUpItemRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRW<PickUpItemRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
            RPCHelper.CreateLocalEvent(new PickUpItemCompleted(){ 
                chunkIndex = rpc.ValueRO.chunkIndex,
                slotIndex = rpc.ValueRO.slotIndex        
             },ecb,player,rpc.ValueRO.networkID,EntityHelper.AddTime(rpc.ValueRO.tick,rpc.ValueRO.duration),false);
        
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);
            ecb.DestroyEntity(e);
        }
        
        foreach ((RefRW<PickUpItemCompleted> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity e) in
        SystemAPI.Query<RefRW<PickUpItemCompleted>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity player = toPlayers.ElementAt(0).connection;
                  
            if(chunks.currentChunks.TryGetValue(rpc.ValueRO.chunkIndex,out Entity chunkEntity) &&
            EQHelper.TryGetPlayerContainer(containersLookup,chunkEntity,EquipmentConfig.chunkItems_ContainerIndex,out var container))
            {
                if(EQHelper.TryGetBufferIndex(slotsLookup,rpc.ValueRO.slotIndex,container.Value.entity,out InventorySlot? inventorySlot,out int bufferIndex))
                {
                    Entity connection = SystemAPI.GetComponent<PlayerSourceConnection>(player).value;
                    GhostChunk chunk = SystemAPI.GetComponent<GhostChunk>(player);

                    SlotPosition slotPosition = new SlotPosition(EquipmentConfig.chunkItems_ContainerIndex,rpc.ValueRO.slotIndex);
                    var tab = EQHelper.FindSlotForItem(componentLookup,slotsLookup,containersLookup,player,inventorySlot.Value,out int remains);
                    var events = EQHelper.MoveItems(ref state,ecb,linkedContainers,barsLookup,slotsLookup,connection,containersLookup,slotPosition,player,chunkEntity,tab);

                    if(events != null) EQHelper.SendEvents(ecb, rpc.ValueRO.networkID, events);

                    if(EQHelper.TryGetBufferIndex(worldItemEntityLookup,rpc.ValueRO.slotIndex,chunkEntity,out var worldItem,out int bufferId))
                        worldItemEntityLookup[chunkEntity].RemoveAtSwapBack(bufferId);

                    if(EQHelper.TryGetBufferIndex(worldItemPositionLookup,rpc.ValueRO.slotIndex,chunkEntity,out var itemPosition,out bufferId))
                        worldItemPositionLookup[chunkEntity].RemoveAtSwapBack(bufferId);

                    if(remains > 0)
                    {
                        float3 position =  SystemAPI.GetComponent<LocalTransform>(player).Position;
                        InventorySlot slot = inventorySlot.Value;
                        slot.quantity = remains;
                        int chunkIndex = rpc.ValueRO.chunkIndex;
                        int newSlot = rpc.ValueRO.slotIndex;

                        if(rpc.ValueRO.chunkIndex != chunk.GetChunk())
                        {
                            if(EQHelper.TryGetPlayerContainer(containersLookup, chunk.currentChunkEntity ,EquipmentConfig.chunkItems_ContainerIndex,out var containerTo)) 
                            {   
                                chunkIndex = chunk.GetChunk();
                                newSlot = EQHelper.GetNextFreeSlotForItem(ref state,slotsLookup,containersLookup,chunk.currentChunkEntity,EquipmentConfig.chunkItems_ContainerIndex);
                                EQHelper.MoveBetweenContainers(ref state,ref ecb,linkedContainers,barsLookup,slotsLookup,Entity.Null,Entity.Null,container.Value,containerTo.Value,newSlot,rpc.ValueRO.slotIndex,int.MaxValue,false,true,true);
                            }
                        }
          
                        RPCHelper.CreateLocalEvent(new SpawnWorldItem(){ 
                            chunkIndex = chunkIndex,
                            slotIndex = newSlot,
                            dropPosition = new float2(position.x,position.y),
                            item = slot,
                            chunk = chunk.currentChunkEntity      
                        },ecb,tick,true);

                    }
                }
            }                

            var buffer = SystemAPI.GetBuffer<FutureEventsForPlayer>(player);
            RPCProcessingSystem.CleanUpEventBuffer(buffer,e);
            ecb.DestroyEntity(e);
        }

        foreach ((RefRO<MergeItems> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<MergeItems>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO.mergeItemsPRC);
            
            Entity chunkFrom;
            Entity chunkTo;
            EntityContainers? containerFrom;
            EntityContainers? containerTo;

            if(rpc.ValueRO.mergeItemsPRC.fromChunkIndex == rpc.ValueRO.mergeItemsPRC.toChunkIndex)
            {
                if(chunks.currentChunks.TryGetValue(rpc.ValueRO.mergeItemsPRC.fromChunkIndex, out var chunkEntity) &&
                EQHelper.TryGetPlayerContainer(containersLookup,chunkEntity,EquipmentConfig.chunkItems_ContainerIndex,out var container))
                {
                    chunkFrom = chunkTo = chunkEntity;
                    containerFrom = containerTo = container.Value;
                }
                else
                    break;
            }
            else
            {
                if(!(chunks.currentChunks.TryGetValue(rpc.ValueRO.mergeItemsPRC.toChunkIndex, out chunkTo) &&
                chunks.currentChunks.TryGetValue(rpc.ValueRO.mergeItemsPRC.fromChunkIndex,out chunkFrom) &&
                EQHelper.TryGetPlayerContainer(containersLookup,chunkTo,EquipmentConfig.chunkItems_ContainerIndex,out containerTo) && 
                EQHelper.TryGetPlayerContainer(containersLookup,chunkFrom,EquipmentConfig.chunkItems_ContainerIndex,out containerFrom)))
                    break;
            } 
            
            EQHelper.MoveBetweenContainers(ref state,ref ecb,linkedContainers,barsLookup,slotsLookup,Entity.Null,Entity.Null,containerFrom.Value,containerTo.Value,rpc.ValueRO.mergeItemsPRC.toSlotIndex,rpc.ValueRO.mergeItemsPRC.fromSlotIndex,moveBetweenObjects: chunkTo != chunkFrom,serverMove:true,ignoreStackMax:true);
            if(EQHelper.TryGetBufferIndex(slotsLookup,rpc.ValueRO.mergeItemsPRC.toSlotIndex,containerTo.Value.entity,out InventorySlot? inventorySlot,out int bufferIndex))
                worldItemComponentLookup.GetRefRW(rpc.ValueRO.worldItemTo).ValueRW.item = inventorySlot.Value;


            if(EQHelper.TryGetBufferIndex(worldItemEntityLookup,rpc.ValueRO.mergeItemsPRC.fromSlotIndex,chunkFrom,out var worldItem,out int bufferId))
                worldItemEntityLookup[chunkFrom].RemoveAtSwapBack(bufferId);

            if(EQHelper.TryGetBufferIndex(worldItemPositionLookup,rpc.ValueRO.mergeItemsPRC.fromSlotIndex,chunkFrom,out var itemPosition,out bufferId))
                worldItemPositionLookup[chunkFrom].RemoveAtSwapBack(bufferId);


            RPCHelper.CreateLocalEvent<MergeItemsCompleted>(new MergeItemsCompleted()
            {
                rpc = rpc.ValueRO
            },ecb,EntityHelper.AddTime(rpc.ValueRO.mergeItemsPRC.tick,rpc.ValueRO.mergeItemsPRC.duration),false);

            ecb.AddComponent<DestroyEntityTag>(rpc.ValueRO.worldItemFrom);
            ecb.DestroyEntity(entity);
        }
        
        foreach ((RefRO<MergeItemsCompleted> rpc, Entity entity) in
        SystemAPI.Query<RefRO<MergeItemsCompleted>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            if(worldItemComponentLookup.EntityExists(rpc.ValueRO.rpc.worldItemTo))
            {
                var worldItem = worldItemComponentLookup.GetRefRW(rpc.ValueRO.rpc.worldItemTo);
                worldItem.ValueRW.mergeCounter--;
                if(worldItem.ValueRW.mergeCounter == 0)
                {
                    worldItemComponentLookup.SetComponentEnabled(rpc.ValueRO.rpc.worldItemTo,true);
                }
            }
            ecb.DestroyEntity(entity);
        }

        foreach ((RefRO<CreateWorldItemRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<CreateWorldItemRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            for(int i = 1; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);
            ecb.DestroyEntity(entity);
        }



        ecb.Playback(state.EntityManager);  
        ecb.Dispose();
    }
}