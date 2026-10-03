using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct NewParticlesSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewParticles>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((var localTransform,var newParticles,var entity) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<NewParticles>>().WithEntityAccess())
        {
            Entity targetPosition = newParticles.ValueRO.target;
            if(SystemAPI.Exists(targetPosition))
            {         
                var trans = SystemAPI.GetComponent<LocalTransform>(targetPosition);
                var position = trans.Position;
                quaternion rotation = trans.Rotation;

                while(SystemAPI.HasComponent<Parent>(targetPosition))
                {
                    targetPosition = SystemAPI.GetComponent<Parent>(targetPosition).Value;
                    var transform = SystemAPI.GetComponent<LocalTransform>(targetPosition);
                    position = math.transform(transform.ToMatrix(), position);
                    rotation = transform.TransformRotation(rotation);
                }

                localTransform.ValueRW.Position = position +  math.rotate(rotation, newParticles.ValueRO.offset);
            }
            entityCommandBuffer.RemoveComponent<NewParticles>(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }         
}


