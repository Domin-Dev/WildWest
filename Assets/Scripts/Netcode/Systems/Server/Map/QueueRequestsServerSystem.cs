using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;


[UpdateAfter(typeof(CalculateChunksForPlayersServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
partial struct QueueRequestsServerSystem : ISystem
{
    
    EntityQuery LoadRequests;
    EntityQuery players; 

    private PriorityQueue<LoadChunkRequest> loadChunkRequests;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest>().Build();
        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        state.RequireForUpdate<MapSettings>();
        state.RequireForUpdate(LoadRequests);
        loadChunkRequests = new PriorityQueue<LoadChunkRequest>(512, Allocator.Persistent);
    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        loadChunkRequests.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var map = SystemAPI.GetSingleton<MapSettings>();
        int playerCount = players.CalculateEntityCount();
    
        foreach ((RefRO<LoadChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            loadChunkRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }

        int loadChunksInTheTick = Math.Min(playerCount * map.loadedChunksInTickPerClient,map.maxLoadedChunksInTick);
        while(loadChunksInTheTick > 0 && loadChunkRequests.TryPop(out var result))
        {
            ecb.AddComponent<ProcessInTheTick>(result.entity);
            loadChunksInTheTick--;
        }
    }
}

   