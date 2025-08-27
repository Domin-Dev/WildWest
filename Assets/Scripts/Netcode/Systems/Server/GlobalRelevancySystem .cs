using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GlobalRelevancySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var ghostRelevancy = SystemAPI.GetSingletonRW<GhostRelevancy>();
        ghostRelevancy.ValueRW.GhostRelevancyMode = GhostRelevancyMode.SetIsRelevant;
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var ghostRelevancy = SystemAPI.GetSingleton<GhostRelevancy>();


        foreach ((RefRO<InterestArea> area, RefRO<LocalTransform> playerPos, RefRO<GhostOwner> ghostOwner, Entity entity)
        in SystemAPI.Query<RefRO<InterestArea>, RefRO<LocalTransform>, RefRO<GhostOwner>>().WithEntityAccess())
        {
            foreach ((RefRO<GhostInstance> ghost, RefRO<LocalTransform> ghostPos, Entity ghostObj)
            in SystemAPI.Query<RefRO<GhostInstance>,RefRO<LocalTransform>>().WithAll<Physics2D>().WithEntityAccess())
            {
                if(entity == ghostObj) continue;
                bool isRelevant = math.distance(MyTools.ConvertFloat(playerPos.ValueRO.Position), MyTools.ConvertFloat(ghostPos.ValueRO.Position)) < area.ValueRO.radius;
                var key = new RelevantGhostForConnection()
                {
                    Ghost = ghost.ValueRO.ghostId,
                    Connection = ghostOwner.ValueRO.NetworkId
                };

                if (isRelevant)
                    ghostRelevancy.GhostRelevancySet.TryAdd(key, 0); 
                else if(ghostRelevancy.GhostRelevancySet.TryGetValue(key,out int item))
                    ghostRelevancy.GhostRelevancySet.Remove(key);

            }
        }

        foreach ((RefRO<GhostOwner> ghostOwner,RefRO<GhostInstance> ghost, Entity entity)
        in SystemAPI.Query<RefRO<GhostOwner>, RefRO<GhostInstance>>().WithAll<SendToPlayer>().WithEntityAccess())
        {
            if(ghost.ValueRO.ghostId == 0) continue;
            var key = new RelevantGhostForConnection()
            {
                Ghost = ghost.ValueRO.ghostId,
                Connection = ghostOwner.ValueRO.NetworkId
            };
            ghostRelevancy.GhostRelevancySet.Add(key, 0);
            entityCommandBuffer.RemoveComponent<SendToPlayer>(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
