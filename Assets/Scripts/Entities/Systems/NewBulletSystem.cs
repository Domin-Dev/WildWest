using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



//[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]

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
                entityCommandBuffer.RemoveComponent<NewBullet>(entity);
                HybridManager.instance.SetEntity(entity, new Vector3(pos.ValueRO.Position.x, pos.ValueRO.Position.y, 100f));
            }

            float3 v3 = pos.ValueRO.Right();
            velocity.ValueRW.Value = new float2(v3.x,v3.y) * bullet.ValueRO.speed;
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}