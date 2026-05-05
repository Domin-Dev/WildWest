using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


partial struct NewBulletSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewBullet>();
        state.RequireForUpdate<Bullet>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<LocalTransform> pos, RefRW<Velocity2D> velocity, RefRO <NewBullet> newbullet, RefRO<Bullet> bullet, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>,RefRW<Velocity2D>, RefRO<NewBullet>, RefRO<Bullet>>().WithAll<Simulate>().WithEntityAccess())
        {
            if(!newbullet.ValueRO.isOnServer)
            {
                HybridManager.instance.SetEntity(entity, new Vector3(pos.ValueRO.Position.x, pos.ValueRO.Position.y, 100f));
            }

            Debug.Log( entity + " raw  _" + pos.ValueRO.Rotation);
            float3 v3 = pos.ValueRO.Right();
            velocity.ValueRW.Value = new float2((float)MathF.Round(v3.x,3),MathF.Round(v3.y,3)) * bullet.ValueRO.speed;
            entityCommandBuffer.RemoveComponent<NewBullet>(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}