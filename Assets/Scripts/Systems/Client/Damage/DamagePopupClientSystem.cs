using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct DamagePopupClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<DamagePopup, Simulate>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((RefRW<DamagePopup> damagePopup, RefRW<LocalTransform> localTransform, Entity e) in SystemAPI.Query<RefRW<DamagePopup>, RefRW<LocalTransform>>().WithAll<Simulate>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            var dt = damagePopup.ValueRW;

            dt.elapsedTime += deltaTime;
            float progress = math.saturate(dt.elapsedTime / dt.lifetime);
            float3 offset = dt.moveDirection * progress;
            localTransform.ValueRW.Position = dt.startPosition + offset;
            Color color = state.EntityManager.GetComponentObject<TextMesh>(e).color;
            color.a = 1 - progress;
            state.EntityManager.GetComponentObject<TextMesh>(e).color = color;

            if (dt.elapsedTime >= dt.lifetime)
            {
                entityCommandBuffer.DestroyEntity(e);
            }

            damagePopup.ValueRW = dt;
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
