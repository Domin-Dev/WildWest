using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

public struct NewWorldItem : IComponentData
{
    
    
}

public struct WorldItem : IComponentData, IEnableableComponent
{
    public int chunkIndex;
    public int slotIndex;
    public int mergeCounter;
    public InventorySlot item;
}


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(WorldItemServerSystem))]
partial struct SpawnWorldItemServerSystem : ISystem
{
    private EntityArchetype worldItemArchetype;
    private BlobAssetReference<Unity.Physics.Collider> collider;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldItemsConfig>();
        if(SystemAPI.TryGetSingleton<WorldItemsConfig>(out var worldItemsConfig))
        {
            worldItemArchetype = state.EntityManager.CreateArchetype(
                typeof(LocalTransform),typeof(PhysicsCollider),typeof(PhysicsWorldIndex),typeof(WorldItem),typeof(NewWorldItem));

            var boxGeometry = new BoxGeometry
            {
                Center = new float3(0,0.1f,0),
                Size = new float3(worldItemsConfig.sizeWorldItemCollider,300f),
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

        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<LoadedChunks>();
        state.RequireForUpdate<Chunks>();
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
            ecb.SetComponentEnabled<WorldItem>(entity,false);
            ecb.SetComponent(entity,LocalTransform.FromPosition(new float3(rpc.ValueRO.dropPosition,rpc.ValueRO.dropPosition.y)));
            ecb.SetComponent(entity, new PhysicsCollider
            {
                Value = collider,
            });
            ecb.SetSharedComponent<PhysicsWorldIndex>(entity,new PhysicsWorldIndex());
            ecb.DestroyEntity(e);
            ecb.AppendToBuffer(rpc.ValueRO.chunk,new WorldItemEntity()
            {
                slot = rpc.ValueRO.slotIndex,
                worldItem = entity
            });
            ecb.AppendToBuffer(rpc.ValueRO.chunk,new WorldItemPosition()
            {
                slot = rpc.ValueRO.slotIndex,
                worldItemPos = rpc.ValueRO.dropPosition
            });
        }
        
        ecb.Playback(state.EntityManager);  
        ecb.Dispose();
    }
}