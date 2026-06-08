

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;



[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
[RequireMatchingQueriesForUpdate]

public partial struct GhostChangesPositionSystem : ISystem
{
    public static event Action<float2> onPlayerMove;    
    
    DynamicBuffer<LoadedChunks> loadedChunks;
    MapSettings map;


    public void OnUpdate(ref SystemState state)
    {
        if(state.World.IsServer())
        {
            SystemAPI.TryGetSingletonBuffer<LoadedChunks>(out loadedChunks);
            map = SystemAPI.GetSingleton<MapSettings>();
        }
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (localTransform, ghostChunk, entity)
        in SystemAPI.Query<RefRW<LocalTransform>, RefRW<GhostChunk>>().WithAll<GhostInstance,PhysicsVelocity,Simulate>().WithEntityAccess())
        {
            float3 delta = localTransform.ValueRO.Position - ghostChunk.ValueRO.lastPosition;

            if (math.lengthsq(delta) > 0.0001f)
            {
                if(state.World.IsServer())
                {    
                    GhostChangeChunk(state.EntityManager,ecb,localTransform.ValueRO.Position,entity, out bool chunkIsLoaded);
                    if(!chunkIsLoaded && ghostChunk.ValueRO.LastPositionIsCorrect())
                        localTransform.ValueRW.Position = ghostChunk.ValueRO.lastPosition;
                    
                    
                    if(state.EntityManager.HasComponent<ToSave>(entity)) 
                        state.EntityManager.SetComponentEnabled<ToSave>(entity,true);      
                }
                else if(SystemAPI.HasComponent<Player>(entity) && SystemAPI.HasComponent<GhostOwnerIsLocal>(entity))
                {
                    onPlayerMove?.Invoke(MyTools.ConvertFloat(localTransform.ValueRO.Position));
                }
                ghostChunk.ValueRW.lastPosition = localTransform.ValueRO.Position;
                float3 pos = localTransform.ValueRO.Position;
                localTransform.ValueRW.Position = new float3(pos.x,pos.y,pos.y);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


    public void GhostChangeChunk(EntityManager entityManager,EntityCommandBuffer entityCommandBuffer, float3 newPos, Entity entity, out bool chunkIsLoaded)
    {
        int index = ChunkManagementServerSystem.Map.settings.GetChunkIndexFromEnginePosition(newPos);
        var chunk = entityManager.GetComponentData<GhostChunk>(entity);
        chunkIsLoaded = true;

        if (chunk.current != index)
        {
            foreach(var loadedChunk in loadedChunks)
            {
                if(loadedChunk.chunkIndex == index)
                {
                    chunk.SetNewChunk(index);
                    entityCommandBuffer.SetComponent(entity, chunk);
                    entityCommandBuffer.SetComponentEnabled<NewChunk>(entity, true);
                    return;
                }
            }
            if(chunk.CurrentChunkIsNull())
            {
                chunk.spawnChunk = index;
                entityCommandBuffer.SetComponent(entity, chunk);
                entityCommandBuffer.SetComponentEnabled<NewChunk>(entity, true);
            }
            if(entityManager.HasComponent<DestroyAtTick>(entity))
            {
                if(map.CheckChunkIndex(index))
                {
                    var destoryTimer = entityManager.GetComponentData<DestroyAtTick>(entity);
                    destoryTimer.tick.Add(1);
                    entityCommandBuffer.SetComponent(entity,destoryTimer);
                }
                else
                {
                    entityCommandBuffer.AddComponent<DestroyEntityTag>(entity);
                }
            }
            chunkIsLoaded = false;
        }
    }
}

