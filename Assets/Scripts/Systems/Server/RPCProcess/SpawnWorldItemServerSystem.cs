using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public struct WorldItem : IComponentData, IEnableableComponent
{
    public int chunkIndex;
    public int slotIndex;
    public InventorySlot item;
}


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(RPCProcessingSystem))]
partial struct SpawnWorldItemServerSystem : ISystem
{

    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<EntityContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;
    private BufferLookup<LinkedContainers> linkedContainers;


    private EntityArchetype worldItemArchetype;
    private BlobAssetReference<Unity.Physics.Collider> collider;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<LoadedChunks>();

        worldItemArchetype = state.EntityManager.CreateArchetype(
            typeof(LocalTransform),typeof(PhysicsCollider),typeof(PhysicsWorldIndex),typeof(WorldItem));

        var boxGeometry = new BoxGeometry
        {
            Center = new float3(0,0.1f,0),
            Size = new float3(0.23f,0.23f,300f),
            Orientation = quaternion.identity,
            BevelRadius = 0f
        };
        var collisionFilter = new CollisionFilter()
        {
            BelongsTo =  1u << 8,
            CollidesWith = 1u << 9
        };
        var material = new Unity.Physics.Material()
        {
            Friction = 0f,
            Restitution = 0f,   
            CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents,
        };
        collider = Unity.Physics.BoxCollider.Create(boxGeometry,collisionFilter,material);
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRW<SpawnWorldItem> rpc, Entity e) in
        SystemAPI.Query<RefRW<SpawnWorldItem>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            var entity = ecb.CreateEntity(worldItemArchetype);
            ecb.SetComponent(entity, new WorldItem()
            {
                slotIndex = rpc.ValueRO.slotIndex,
                chunkIndex = rpc.ValueRO.chunkIndex,
                item = rpc.ValueRO.item
            });
            ecb.SetComponent(entity,LocalTransform.FromPosition(new float3(rpc.ValueRO.dropPosition,rpc.ValueRO.dropPosition.y)));
            ecb.SetComponent(entity, new PhysicsCollider
            {
                Value = collider,
            });
            ecb.SetSharedComponent<PhysicsWorldIndex>(entity,new PhysicsWorldIndex());
            ecb.DestroyEntity(e);
            ecb.AppendToBuffer(rpc.ValueRO.chunk,new WorldItems()
            {
                slot = rpc.ValueRO.slotIndex,
                worldItem = entity
            });
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}