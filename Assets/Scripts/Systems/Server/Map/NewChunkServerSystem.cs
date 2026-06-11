using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using UnityEngine;


public struct ClientChunks : IComponentData
{
    public  NativeHashMap<int,Entity> currentChunks;
}


[UpdateInGroup(typeof(MapSystemGroup),OrderFirst = true)]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class NewChunSystem : SystemBase
{
    EntityQuery query;
    ChunkManagementServerSystem chunkManagerSystem;
    NativeHashMap<int,Entity> currentChunks;

    [BurstCompile]
    protected override void OnCreate()
    {
        query = SystemAPI.QueryBuilder().WithAll<ChunkComponent,NewChunk,Simulate>().Build();
        if(World.IsServer()) 
        {
            chunkManagerSystem = World.GetExistingSystemManaged<ChunkManagementServerSystem>();
            RequireForUpdate<MapSettings>();
        }
        else
        {
            currentChunks = new NativeHashMap<int, Entity>(32,Allocator.Persistent);
            var entity = World.EntityManager.CreateEntity(typeof(ClientChunks));
            SystemAPI.SetComponent(entity,new ClientChunks()
            {
                currentChunks = currentChunks
            });
        }

        RequireForUpdate(query);
;    
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        currentChunks.Dispose();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

        foreach ((RefRO<ChunkComponent> chunkComponent, Entity entity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithAll<NewChunk,Simulate>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            ecb.AddBuffer<WorldItems>(entity);
            ecb.AddComponent(entity,new ChunkComponentCleanUp() {chunkIndex = chunkComponent.ValueRO.chunkIndex});
            
            ecb.RemoveComponent<NewChunk>(entity);
            if(World.IsServer())
            {
                ChunkManagementServerSystem.loadedChunks.TryAdd(chunkComponent.ValueRO.chunkIndex,new LoadedChunks()
                {
                    chunkEntity = entity,
                    chunkIndex = chunkComponent.ValueRO.chunkIndex,
                });
            }
            else
            {
                if(currentChunks.ContainsKey(chunkComponent.ValueRO.chunkIndex))
                    currentChunks[chunkComponent.ValueRO.chunkIndex] = entity;
                else
                    currentChunks.Add(chunkComponent.ValueRO.chunkIndex,entity);
            }
        }
    }
}

   