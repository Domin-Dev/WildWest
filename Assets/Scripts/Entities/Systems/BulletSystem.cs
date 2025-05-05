using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]

partial struct BulletSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<Bullet>();
    }
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> worldPos, RefRO <Bullet> bullet) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>>().WithAll<Simulate>())
        {
            worldPos.ValueRW.Position += new float3(-1, 0, 0) * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;
        }
    }

}