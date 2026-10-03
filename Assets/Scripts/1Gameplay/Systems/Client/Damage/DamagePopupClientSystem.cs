using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[RequireMatchingQueriesForUpdate]
partial struct DamagePopupClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((RefRW<DamagePopup> damagePopup, RefRW<LocalTransform> localTransform, Entity e) in SystemAPI.Query<RefRW<DamagePopup>, RefRW<LocalTransform>>().WithAll<Simulate>().WithEntityAccess())
        {
            var dt = damagePopup.ValueRW;

            dt.elapsedTime += deltaTime;
            float progress = math.saturate(dt.elapsedTime / dt.lifetime);
            float3 offset = dt.moveDirection * progress;
            localTransform.ValueRW.Position = dt.startPosition + offset;
            var textmesh = state.EntityManager.GetComponentObject<TextMesh>(e);
            Color color = textmesh.color;
            color.a = 1 - progress;
            textmesh.color = color;

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
