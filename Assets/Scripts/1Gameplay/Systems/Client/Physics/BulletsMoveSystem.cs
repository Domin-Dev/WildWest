using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(CopyCommandBufferToInputSystemGroup),OrderLast = true)]
[RequireMatchingQueriesForUpdate]
public partial struct BulletsMoveSystem : ISystem
{

    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("update bullet!!!");
        foreach (var (transform, velocity, bullet)
            in SystemAPI.Query<RefRO<LocalTransform>,RefRW<PhysicsVelocity>, RefRO<Bullet>>().WithAll<Simulate>().WithNone<DestroyEntityTag>().WithDisabled<NewBullet>())
        {
            float3 v3 = transform.ValueRO.Right();
            velocity.ValueRW.Linear = new float3((float)MathF.Round(v3.x,3),MathF.Round(v3.y,3),0) * bullet.ValueRO.speed;
        }
    }
}