using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[UpdateAfter(typeof(CalculateChunksForPlayersServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
partial struct QueueRequestsServerSystem : ISystem
{

    EntityQuery players; 

    private PriorityQueue<LoadChunkRequest> loadChunkRequests;
    private PriorityQueue<StopSendingChunkRequest> stopSendingRequests;
    private PriorityQueue<StartSendingChunkRequest> startSendingRequests;

    private int playerCount;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQuery LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest>().Build();
        EntityQuery stopRequests = SystemAPI.QueryBuilder().WithAll<StopSendingChunkRequest>().Build();
        EntityQuery startRequests = SystemAPI.QueryBuilder().WithAll<StartSendingChunkRequest>().Build();


        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        state.RequireForUpdate<MapSettings>();
        NativeArray<EntityQuery> entityQueries = new NativeArray<EntityQuery>(3,Allocator.Temp);
        entityQueries[0] = LoadRequests;
        entityQueries[1] = stopRequests;
        entityQueries[2] = startRequests;
        state.RequireAnyForUpdate(entityQueries);


        loadChunkRequests = new PriorityQueue<LoadChunkRequest>(256, Allocator.Persistent);
        stopSendingRequests = new PriorityQueue<StopSendingChunkRequest>(256,Allocator.Persistent);
        startSendingRequests = new PriorityQueue<StartSendingChunkRequest>(256,Allocator.Persistent);
        
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
   
        //Start Requests
        foreach ((RefRO<StartSendingChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<StartSendingChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            startSendingRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }
        ProcessRequests(ecb,ref startSendingRequests,map.startRequestsInTickPerClient,map.maxStartRequestsInTick);   
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

   