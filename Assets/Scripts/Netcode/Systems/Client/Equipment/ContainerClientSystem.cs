using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct ContainerClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ContainerComponent>().WithNone<ContainerLoaded>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ContainerComponent> containerComponent, Entity entity) in SystemAPI.Query<RefRO<ContainerComponent>>().WithNone<ContainerLoaded>().WithEntityAccess())
        {
            NewEquipmentManager.instance.LoadContainer(containerComponent.ValueRO,entity);
            entityCommandBuffer.AddComponent<ContainerLoaded>(entity);
        }
        
        
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
