
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup),OrderLast = true)]

partial struct MoveToTargetSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoveToTarget>();
        state.RequireForUpdate<NetworkTime>();
    }
    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        int simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        ComponentLookup<LocalTransform> positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        foreach((RefRO<MoveToTarget> moveToTarget,RefRW<LocalTransform> position ,Entity entity) in SystemAPI.Query<RefRO<MoveToTarget>,RefRW<LocalTransform>>().WithEntityAccess())
        {
            int lifetimeInTicks = (int)(moveToTarget.ValueRO.duration * simulationTickRate);
            int tickSince = networkTime.ServerTick.TicksSince(moveToTarget.ValueRO.startTick);
            float progress =(float)tickSince/lifetimeInTicks;
            float2 target = moveToTarget.ValueRO.target;
            if(moveToTarget.ValueRO.targetEntity != Entity.Null)
            {
                if(positionLookup.TryGetComponent(moveToTarget.ValueRO.targetEntity,out var transform))
                    target = new float2(transform.Position.x,transform.Position.y);
            }

            float2 pos = math.lerp(moveToTarget.ValueRO.startPosition,target,progress);
            position.ValueRW.Position = new float3(pos.x,pos.y,pos.y);
            if(progress >= 1)
            {
                ecb.RemoveComponent<MoveToTarget>(entity);   
                if(moveToTarget.ValueRO.destroy)
                    ecb.AddComponent<DestroyEntityTag>(entity);
            }    
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
} 