using Unity.Entities;
using UnityEngine;


public struct EntityToHide : IComponentData{}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct HidingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntityToHide>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((EntityToHide entityToHide,Entity entity)
        in SystemAPI.Query<EntityToHide>().WithEntityAccess())
        {
            state.EntityManager.GetComponentObject<SpriteRenderer>(entity).enabled = false;
            entityCommandBuffer.RemoveComponent<EntityToHide>(entity);    
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}