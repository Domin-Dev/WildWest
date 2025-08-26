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
        ghostRelevancy.ValueRW.GhostRelevancyMode = GhostRelevancyMode.SetIsIrrelevant;
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var ghostRelevancy = SystemAPI.GetSingleton<GhostRelevancy>();


        foreach ((RefRO<InterestArea> area, RefRO<LocalTransform> playerPos, RefRO<GhostOwner> ghostOwner, Entity entity)
        in SystemAPI.Query<RefRO<InterestArea>, RefRO<LocalTransform>, RefRO<GhostOwner>>().WithEntityAccess())
        {
            foreach ((RefRO<GhostInstance> ghost, RefRO<LocalTransform> ghostPos, Entity ghostObj)
            in SystemAPI.Query<RefRO<GhostInstance>,RefRO<LocalTransform>>().WithEntityAccess())
            {
                if(entity == ghostObj) continue;
                bool isRelevant = math.distance(MyTools.ConvertFloat(playerPos.ValueRO.Position), MyTools.ConvertFloat(ghostPos.ValueRO.Position)) < area.ValueRO.radius;
                var key = new RelevantGhostForConnection()
                {
                    Ghost = ghost.ValueRO.ghostId,
                    Connection = ghostOwner.ValueRO.NetworkId
                };

                Debug.Log(isRelevant + " " + ghostObj);

                if (!isRelevant)
                    ghostRelevancy.GhostRelevancySet.TryAdd(key, 0); 
                else if(ghostRelevancy.GhostRelevancySet.TryGetValue(key,out int item))
                    ghostRelevancy.GhostRelevancySet.Remove(key);

            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
