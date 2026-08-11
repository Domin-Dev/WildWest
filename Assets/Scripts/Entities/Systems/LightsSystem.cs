
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct LightsSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LinkedLight>();
    }
    public void OnUpdate(ref SystemState state)
    {
        if(HybridManager.instance == null)  return;

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach ((RefRO<LinkedLight> light,Entity entity) in SystemAPI.Query<RefRO<LinkedLight>>().WithEntityAccess())
        {
            TransformFollower follower = new TransformFollower()
            {
                follower = HybridManager.instance.GetLight(light.ValueRO),
                offset = light.ValueRO.Offset
            };

            if(!SystemAPI.HasBuffer<TransformFollower>(entity))
                entityCommandBuffer.AddBuffer<TransformFollower>(entity);
                
            entityCommandBuffer.AppendToBuffer(entity,follower); 
            entityCommandBuffer.RemoveComponent<LinkedLight>(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }
}



