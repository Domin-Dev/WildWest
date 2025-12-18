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
    EntityQuery stopRequests;
    EntityQuery players; 

    private PriorityQueue<LoadChunkRequest> loadChunkRequests;
    private PriorityQueue<StopSendingChunkRequest> stopSendingRequests;

    private int playerCount;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest>().Build();
        stopRequests = SystemAPI.QueryBuilder().WithAll<StopSendingChunkRequest>().Build();


        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        state.RequireForUpdate<MapSettings>();
        NativeArray<EntityQuery> entityQueries = new NativeArray<EntityQuery>(2,Allocator.Temp);
        entityQueries[0] = LoadRequests;
        entityQueries[1] = stopRequests;
        state.RequireAnyForUpdate(entityQueries);


        loadChunkRequests = new PriorityQueue<LoadChunkRequest>(256, Allocator.Persistent);
        stopSendingRequests = new PriorityQueue<StopSendingChunkRequest>(256,Allocator.Persistent);
        entityQueries.Dispose();
    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        loadChunkRequests.Dispose();
        stopSendingRequests.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("dzial!!!!");
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var map = SystemAPI.GetSingleton<MapSettings>();
        playerCount = players.CalculateEntityCount();
    

        // Load Requests 
        foreach ((RefRO<LoadChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            loadChunkRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }
         ProcessRequests(ecb,ref loadChunkRequests,map.loadedChunksInTickPerClient,map.maxLoadedChunksInTick);    
        //Stop Requests
       
        foreach ((RefRO<StopSendingChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<StopSendingChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            stopSendingRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }
        ProcessRequests(ecb,ref stopSendingRequests,map.stopRequestsInTickPerClient,map.maxStopRequestsInTick);
    }


    private void ProcessRequests<T>(EntityCommandBuffer ecb,ref PriorityQueue<T> queue,int requestsInTickPerClient, int maxRequestsInTick) where T : unmanaged,IPriority
    {
        int requestsInTheTick = Math.Min(playerCount * requestsInTickPerClient,maxRequestsInTick);
        while(requestsInTheTick > 0 && queue.TryPop(out var result))
        {
            ecb.AddComponent<ProcessInTheTick>(result.entity);
            requestsInTheTick--;
        }
    }
}

   