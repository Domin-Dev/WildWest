using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(RPCProcessingSystem))]
partial struct WorldItemServerSystem : ISystem
{

    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<EntityContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;
    private BufferLookup<LinkedContainers> linkedContainers;
    private ComponentLookup<ContainerComponent> componentLookup;
    private BufferLookup<WorldItems> worldItemsLookup;


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
        worldItemsLookup = SystemAPI.GetBufferLookup<WorldItems>();

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
        worldItemsLookup.Update(ref state);

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
                        RPCHelper.CreateSerwerLocalEvent(new SpawnWorldItem(){ 
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

                    if(EQHelper.TryGetBufferIndex(worldItemsLookup,rpc.ValueRO.slotIndex,chunkEntity,out var worldItem,out int bufferId))
                        worldItemsLookup[chunkEntity].RemoveAtSwapBack(bufferId);

                    if(remains > 0)
                    {
                        float3 position =  SystemAPI.GetComponent<LocalTransform>(player).Position;
                        InventorySlot slot = inventorySlot.Value;
                        slot.quantity = remains;
                        int chunkIndex = rpc.ValueRO.chunkIndex;
                        int newSlot = rpc.ValueRO.slotIndex;


                        Debug.Log(rpc.ValueRO.chunkIndex + "   zostalo " + chunk.GetChunk() + " " + chunk.currentChunkEntity);
                        if(rpc.ValueRO.chunkIndex != chunk.GetChunk())
                        {
                            if(EQHelper.TryGetPlayerContainer(containersLookup, chunk.currentChunkEntity ,EquipmentConfig.chunkItems_ContainerIndex,out var containerTo)) 
                            {   
                                Debug.Log("zostalo!!! +" + containerTo.Value.entity + " " + containerTo.Value.index);
                                chunkIndex = chunk.GetChunk();
                                newSlot = EQHelper.GetNextFreeSlotForItem(ref state,slotsLookup,containersLookup,chunk.currentChunkEntity,EquipmentConfig.chunkItems_ContainerIndex);
                                EQHelper.MoveBetweenContainers(ref state,ref ecb,linkedContainers,barsLookup,slotsLookup,Entity.Null,Entity.Null,container.Value,containerTo.Value,newSlot,rpc.ValueRO.slotIndex,int.MaxValue,false,true,true);
                            }
                        }
                        
                        RPCHelper.CreateSerwerLocalEvent(new SpawnWorldItem(){ 
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
        
        ecb.Playback(state.EntityManager);  
        ecb.Dispose();
    }
}