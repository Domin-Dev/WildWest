
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;


public struct EQSpawnItem : IComponentData
{
    public InventorySlot item;
    public float barValue;
    public int chunkIndex;
    public float2 position;
    public float2 fromPosition;
    public float duration;
}


[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct SpawnItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<LinkedContainers> linkedContainersLookup;



    private BufferLookup<PlayersNeedChunk> needLookup;
    private ComponentLookup<PlayerSourceConnection> connections;




    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<Chunks>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<EQSpawnItem>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedContainersLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
        needLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
        connections = SystemAPI.GetComponentLookup<PlayerSourceConnection>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedContainersLookup.Update(ref state);
        needLookup.Update(ref state);
        connections.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        Chunks chunks = SystemAPI.GetSingleton<Chunks>();
        NetworkTick tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var loaded = SystemAPI.GetSingletonBuffer<LoadedChunks>();

        
        foreach ((RefRO<EQSpawnItem> command, Entity entity) in
        SystemAPI.Query<RefRO<EQSpawnItem>>().WithEntityAccess())
        {
            if(chunks.currentChunks.TryGetValue(command.ValueRO.chunkIndex,out Entity chunkEntity))
            {
                var container = EQHelper.GetContainer(playerContainersLookup, chunkEntity, EquipmentConfig.chunkItems_ContainerIndex);
                if(container.HasValue)
                {
                    int slotIndex = EQHelper.GetNextFreeSlotForItem(ref state,slotsLookup,playerContainersLookup,chunkEntity,EquipmentConfig.chunkItems_ContainerIndex);
                    var args = new [] {new EQTransferData(){
                        pos = new SlotPosition(EquipmentConfig.chunkItems_ContainerIndex,slotIndex),
                        quantity = command.ValueRO.item.quantity,
                        slotExist = false
                    }};
                    EQHelper.AddItems(ref state,entityCommandBuffer,ref entitiesReferences,linkedContainersLookup,barsLookup,slotsLookup, playerContainersLookup,chunkEntity,args,command.ValueRO.item,command.ValueRO.barValue);



                    // RPCHelper.CreateLocalEvent(new SpawnWorldItem(){ 
                    //     chunkIndex = command.ValueRO.chunkIndex,
                    //     slotIndex =  slotIndex,
                    //     dropPosition = command.ValueRO.position,
                    //     item = command.ValueRO.item,
                    //     chunk = chunkEntity    
                    // },entityCommandBuffer,tick,true);

                    RPCHelper.SendEventsToClients(new DropItemRPC()
                    {
                        chunkIndex = command.ValueRO.chunkIndex,
                        dropPosition = command.ValueRO.position,
                        fromPosition = command.ValueRO.fromPosition,
                        duration = command.ValueRO.duration,
                        networkID = 0,
                        slotIndex = slotIndex,
                        tick = tick
                    },connections,needLookup,loaded,entityCommandBuffer,command.ValueRO.chunkIndex,tick,true);
                }
                
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
