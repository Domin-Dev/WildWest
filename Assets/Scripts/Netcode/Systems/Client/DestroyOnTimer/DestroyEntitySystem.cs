using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
[BurstCompile]
public partial class DestroyEntitySystem : SystemBase
{

    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        RequireForUpdate<NetworkTime>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
       .WithAll<DestroyEntityTag,Simulate>();
        RequireForUpdate(GetEntityQuery(entityQueryBuilder));
    }
    [BurstCompile]
    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        var current = networkTime.ServerTick;    
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);


        foreach (var(localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<DestroyEntityTag,Simulate>().WithEntityAccess())
        {
            if(World.IsServer())
            {
                if (SystemAPI.HasComponent<Bullet>(entity)) HybridManager.instance.EntityDeleted(entity);
                if (SystemAPI.HasComponent<GhostInstance>(entity)) GlobalRelevancySystem.OnGhostDestroyed(EntityManager.GetComponentData<GhostInstance>(entity).ghostId);
                if (SystemAPI.HasComponent<GhostChunk>(entity))
                {
                    var ghostChunk = SystemAPI.GetComponentRO<GhostChunk>(entity);
                    
                }
                
                ecb.DestroyEntity(entity);
            }
            else
            {
                if(SystemAPI.HasComponent<Bullet>(entity)) HybridManager.instance.EntityDeleted(entity);  
                localTransform.ValueRW.Position = new float3(100000, 100000,100000);
                ecb.RemoveComponent<Simulate>(entity);
                if (!EntityManager.HasComponent<GhostInstance>(entity))
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}

