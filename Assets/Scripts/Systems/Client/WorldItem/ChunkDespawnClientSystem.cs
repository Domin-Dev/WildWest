using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

partial struct ChunkDespawnClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Chunks>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ChunkComponentCleanUp>().WithNone<ChunkComponent>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {
        var clientChunks = SystemAPI.GetSingleton<Chunks>();
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((DynamicBuffer<WorldItemEntity> worldItems,RefRO<ChunkComponentCleanUp> chunkindex ,Entity entity) in SystemAPI.Query<DynamicBuffer<WorldItemEntity>,RefRO<ChunkComponentCleanUp>>().WithNone<ChunkComponent>().WithEntityAccess())
        {
            foreach(var i in worldItems)
            {
                if(SystemAPI.Exists(i.worldItem))   
                    ecb.AddComponent<DestroyEntityTag>(i.worldItem);
            }

            clientChunks.currentChunks.Remove(chunkindex.ValueRO.chunkIndex);        
            ecb.RemoveComponent<WorldItemEntity>(entity);
            ecb.RemoveComponent<ChunkComponentCleanUp>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
} 