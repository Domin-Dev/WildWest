using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
partial struct ChunkDespawnClientSystem : ISystem
{

    private ClientChunks clientChunks;
    public void OnUpdate(ref SystemState state)
    {
        if(state.World.IsClient())
        {
            clientChunks = SystemAPI.GetSingleton<ClientChunks>();
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((DynamicBuffer<WorldItems> worldItems,RefRO<ChunkComponentCleanUp> chunkindex ,Entity entity) in SystemAPI.Query<DynamicBuffer<WorldItems>,RefRO<ChunkComponentCleanUp>>().WithNone<ChunkComponent>().WithEntityAccess())
        {
            foreach(var i in worldItems)
            {
                if(SystemAPI.Exists(i.worldItem))   
                    ecb.AddComponent<DestroyEntityTag>(i.worldItem);
            }
            if(state.World.IsClient())
            {
                clientChunks.currentChunks.Remove(chunkindex.ValueRO.chunkIndex);
            }
            ecb.RemoveComponent<WorldItems>(entity);
            ecb.RemoveComponent<ChunkComponentCleanUp>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
} 