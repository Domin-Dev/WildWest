using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;



[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup),OrderFirst = true)]
[RequireMatchingQueriesForUpdate]
public partial class NewChunkServerSystem : SystemBase
{
    EntityQuery query;
    ChunkManagementServerSystem chunkManagerSystem;
    
    [BurstCompile]
    protected override void OnCreate()
    {
        query = SystemAPI.QueryBuilder().WithAll<ChunkComponent,NewChunk,Simulate>().Build();
        chunkManagerSystem = World.GetExistingSystemManaged<ChunkManagementServerSystem>();
        RequireForUpdate(query);
        RequireForUpdate<MapSettings>();    
    }


    [BurstCompile]
    protected override void OnDestroy()
    {
        
    }

    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
        foreach ((RefRO<ChunkComponent> chunkComponent, Entity entity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithAll<NewChunk,Simulate>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            chunkManagerSystem.loadedChunks.TryAdd(chunkComponent.ValueRO.index,new LoadedChunks()
            {
                chunkEntity = entity,
                chunkIndex = chunkComponent.ValueRO.index,
            });
            ecb.RemoveComponent<NewChunk>(entity);
        }
    }
}

   