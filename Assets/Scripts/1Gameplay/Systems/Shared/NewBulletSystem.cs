using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
partial struct NewBulletSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<LocalTransform> pos, RefRW<PhysicsMass> mass, EnabledRefRW<NewBullet> newbullet, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>,RefRW<PhysicsMass>, EnabledRefRW<NewBullet>>().WithAll<Simulate,Bullet>().WithEntityAccess())
        {
            if(state.World.IsClient())
            {
                HybridManager.instance.SetEntity(entity, new Vector3(pos.ValueRO.Position.x, pos.ValueRO.Position.y, 100f));
            }
            mass.ValueRW.InverseInertia = float3.zero;
            newbullet.ValueRW = false;
        }       
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}