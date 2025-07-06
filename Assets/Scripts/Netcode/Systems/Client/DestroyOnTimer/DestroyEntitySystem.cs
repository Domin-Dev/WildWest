using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
public partial struct DestroyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
       .WithAll<DestroyEntityTag,Simulate>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        var current = networkTime.ServerTick;    
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);


        foreach (var(localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<DestroyEntityTag,Simulate>().WithEntityAccess())
        {
            if(state.World.IsServer())
            {
                if (SystemAPI.HasComponent<Bullet>(entity)) HybridManager.instance.EntityDeleted(entity);
                entityCommandBuffer.DestroyEntity(entity);
            }
            else
            {
                if(SystemAPI.HasComponent<Bullet>(entity)) HybridManager.instance.EntityDeleted(entity);  
                localTransform.ValueRW.Position = new float3(100000, 100000,100000);
                entityCommandBuffer.RemoveComponent<Simulate>(entity);
                if (!state.EntityManager.HasComponent<GhostInstance>(entity))
                {
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }
        }
    }
}

