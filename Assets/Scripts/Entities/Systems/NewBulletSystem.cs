using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(CharacterAim))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]

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

        foreach ((RefRO<LocalTransform> pos, RefRO <Bullet> bullet, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Bullet>>().WithAll<NewBullet,Simulate>().WithEntityAccess())
        {
            if(bullet.ValueRO.time >= 0)
            {
                entityCommandBuffer.RemoveComponent<NewBullet>(entity);
                HybridManager.instance.SetEntity(entity, new Vector3(pos.ValueRO.Position.x, pos.ValueRO.Position.y, 100f));
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}