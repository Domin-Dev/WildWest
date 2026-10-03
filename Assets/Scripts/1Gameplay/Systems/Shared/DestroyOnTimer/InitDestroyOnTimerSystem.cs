using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;

public partial struct InitDestroyOnTimerSystem : ISystem
{

    int simulationTickRate;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();                
        simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        

        foreach ((RefRO<DestroyOnTimer> destroyOnTimer, Entity entity) in SystemAPI.Query<RefRO<DestroyOnTimer>>().WithNone<DestroyAtTick>().WithEntityAccess())
        {
            uint lifetimeInTicks = (uint)(destroyOnTimer.ValueRO.value * simulationTickRate);
            var targetTick = currentTick;
            targetTick.Add(lifetimeInTicks);
            entityCommandBuffer.AddComponent(entity, new DestroyAtTick() { tick = targetTick });
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}

