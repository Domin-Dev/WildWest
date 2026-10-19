using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
public partial struct DestroyOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<DestroyAtTick,Simulate>().WithNone<DestroyEntityTag>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach (var (destoryAtTick, entity) in SystemAPI.Query<RefRO<DestroyAtTick>>().WithAll<Simulate>().WithNone<DestroyEntityTag>().WithEntityAccess())
        {
            if(!destoryAtTick.ValueRO.tick.IsNewerThan(currentTick))
                entityCommandBuffer.AddComponent<DestroyEntityTag>(entity);
        }
    }
}

