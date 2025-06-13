using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct DestroyOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach (var (destoryAtTick, entity) in SystemAPI.Query<RefRO<DestroyAtTick>>().WithAll<Simulate>().WithNone<DestroyEntityTag>().WithEntityAccess())
        {
            if (currentTick.Equals(destoryAtTick.ValueRO.tick) || currentTick.IsNewerThan(destoryAtTick.ValueRO.tick))
            {
                entityCommandBuffer.AddComponent<DestroyEntityTag>(entity);
            }
        }
    }
}

