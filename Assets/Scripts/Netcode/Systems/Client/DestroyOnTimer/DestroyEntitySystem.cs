using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;
using Unity.Transforms;
using Unity.Mathematics;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
public partial struct DestroyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
    }


    [BurstCompile]
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
                entityCommandBuffer.DestroyEntity(entity);
            }
            else
            {
                localTransform.ValueRW.Position = new float3(100000, 100000,100000);
            }
        }
    }
}

