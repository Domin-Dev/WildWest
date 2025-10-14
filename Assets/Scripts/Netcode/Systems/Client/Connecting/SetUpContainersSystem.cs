using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct SetUpContainersSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ContainerComponent>().WithNone<GhostChildEntity>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRO<ContainerComponent> container, Entity e) in SystemAPI.Query< RefRO<ContainerComponent>>().WithEntityAccess())
        {
            foreach ((RefRO<Player> ghost, Entity player) in SystemAPI.Query<RefRO<Player>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
            {
                entityCommandBuffer.AppendToBuffer<GhostGroup>(player, new GhostGroup() { Value = e});
            }
            entityCommandBuffer.AddComponent(e, new GhostChildEntity());
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
