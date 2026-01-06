using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[UpdateAfter(typeof(CalculateChunksForPlayersServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial class QueueRequestsServerSystem : SystemBase
{

    EntityQuery players; 

    private PriorityQueue<LoadChunkRequest> loadChunkRequests;
    private PriorityQueue<StopSendingChunkRequest> stopSendingRequests;
    private PriorityQueue<StartSendingChunkRequest> startSendingRequests;
    private NativeQueue<(UnloadChunkRequest request, Entity entity)> unloadChunkRequests;


    private NativeHashSet<int> lastLoadChunkRequests;
    private NativeParallelMultiHashMap<int,int> playersChunks;

    private int playerCount;

    ChunkManagementServerSystem chunkManagerSystem;

    [BurstCompile]
    protected override void OnCreate()
    {
        EntityQuery LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest>().Build();
        EntityQuery stopRequests = SystemAPI.QueryBuilder().WithAll<StopSendingChunkRequest>().Build();
        EntityQuery startRequests = SystemAPI.QueryBuilder().WithAll<StartSendingChunkRequest>().Build();
        EntityQuery unloadRequests = SystemAPI.QueryBuilder().WithAll<UnloadChunkRequest>().Build();

        chunkManagerSystem = World.GetExistingSystemManaged<ChunkManagementServerSystem>();


        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        RequireForUpdate<TickLimitsConfig>();
        NativeArray<EntityQuery> entityQueries = new NativeArray<EntityQuery>(4,Allocator.Temp);
        entityQueries[0] = LoadRequests;
        entityQueries[1] = stopRequests;
        entityQueries[2] = startRequests;
        entityQueries[3] = unloadRequests;

        RequireAnyForUpdate(entityQueries);


        loadChunkRequests = new PriorityQueue<LoadChunkRequest>(128, Allocator.Persistent);
        stopSendingRequests = new PriorityQueue<StopSendingChunkRequest>(128,Allocator.Persistent);
        startSendingRequests = new PriorityQueue<StartSendingChunkRequest>(128,Allocator.Persistent);
        unloadChunkRequests = new NativeQueue<(UnloadChunkRequest request, Entity entity)>(Allocator.Persistent);
        
        lastLoadChunkRequests = new NativeHashSet<int>(256,Allocator.Persistent);
        playersChunks = new NativeParallelMultiHashMap<int, int>(512,Allocator.Persistent);


        entityQueries.Dispose();
    }


    [BurstCompile]
    protected override void OnDestroy()
    {
        loadChunkRequests.Dispose();
        stopSendingRequests.Dispose();
        startSendingRequests.Dispose();
        unloadChunkRequests.Dispose();

        lastLoadChunkRequests.Dispose();
        playersChunks.Dispose();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
        var tickLimits = SystemAPI.GetSingleton<TickLimitsConfig>();
        playerCount = players.CalculateEntityCount();
    

        //Load Requests 
        foreach ((RefRO<LoadChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            if(lastLoadChunkRequests.Contains(requestData.ValueRO.chunkIndex))
            {
                var values = playersChunks.GetValuesForKey(requestData.ValueRO.networkID);
                bool playerHasChunk = false;
                foreach(int chunk in values)
                {
                    if(chunk == requestData.ValueRO.chunkIndex)
                    {
                        playerHasChunk = true;
                        break;
                    }
                }
                values.Dispose();
                if(!playerHasChunk && chunkManagerSystem.loadedChunks.TryGetValue(requestData.ValueRO.chunkIndex,out var loaded))
                {
                    var newRequest = ecb.CreateEntity();
                    ecb.AddComponent(newRequest,new StartSendingChunkRequest(requestData.ValueRO,loaded.chunkEntity));
                    ecb.DestroyEntity(entity);
                    continue;
                }
                
                if(playerHasChunk)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }
            }

            if(requestData.ValueRO.networkID >= 0)
                playersChunks.Add(requestData.ValueRO.networkID,requestData.ValueRO.chunkIndex);
            lastLoadChunkRequests.Add(requestData.ValueRO.chunkIndex);

            loadChunkRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }
        ProcessRequests(ecb,ref loadChunkRequests,tickLimits.loadedChunksInTickPerClient,tickLimits.maxLoadedChunksInTick);    
        
        //Stop Requests
        foreach ((RefRO<StopSendingChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<StopSendingChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            if(!lastLoadChunkRequests.Contains(requestData.ValueRO.chunkIndex))
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            playersChunks.Remove(requestData.ValueRO.networkID,requestData.ValueRO.chunkIndex);
            stopSendingRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }
        ProcessRequests(ecb,ref stopSendingRequests,tickLimits.stopRequestsInTickPerClient,tickLimits.maxStopRequestsInTick);
   
        //Start Requests
        foreach ((RefRO<StartSendingChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<StartSendingChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {        
            if(!lastLoadChunkRequests.Contains(requestData.ValueRO.chunkIndex))
            {
                var newRequest = ecb.CreateEntity();
                ecb.AddComponent(newRequest,new LoadChunkRequest(requestData.ValueRO));
                ecb.DestroyEntity(entity);
                continue;
            }

            playersChunks.Add(requestData.ValueRO.networkID,requestData.ValueRO.chunkIndex);
            startSendingRequests.Push(requestData.ValueRO,entity);
            ecb.AddComponent<QueuedRequest>(entity);
        }
        ProcessRequests(ecb,ref startSendingRequests,tickLimits.startRequestsInTickPerClient,tickLimits.maxStartRequestsInTick);   

        //Unload Requests
        foreach ((RefRO<UnloadChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<UnloadChunkRequest>>().WithNone<QueuedRequest>().WithEntityAccess())
        {
            if(!lastLoadChunkRequests.Contains(requestData.ValueRO.chunkIndex))
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            bool chunkIsIdle = true;
            Debug.Log("dz " +  requestData.ValueRO.chunkIndex);
            foreach(var chunkPlayer in playersChunks)
            {
                if(chunkPlayer.Value == requestData.ValueRO.chunkIndex)
                {
                    chunkIsIdle = false;
                    break;
                }
            }
            if(!chunkIsIdle)
            {
                ecb.DestroyEntity(entity);
                continue;
            }
            lastLoadChunkRequests.Remove(requestData.ValueRO.chunkIndex);
            unloadChunkRequests.Enqueue((requestData.ValueRO,entity));
            chunkManagerSystem.loadedChunks.Remove(requestData.ValueRO.chunkIndex);

            ecb.AddComponent<QueuedRequest>(entity);
        }
        ProcessRequests(ecb,ref unloadChunkRequests,tickLimits.unloadedChunksInTickPerClient,tickLimits.maxUnloadedChunksInTick);   
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

    private void ProcessRequests<T>(EntityCommandBuffer ecb,ref NativeQueue<(T value,Entity entity)> queue,int requestsInTickPerClient, int maxRequestsInTick) where T : unmanaged,IPriority
    {
        int requestsInTheTick = Math.Min(playerCount * requestsInTickPerClient,maxRequestsInTick);
        while(requestsInTheTick > 0 && queue.TryDequeue(out var result))
        {
            ecb.AddComponent<ProcessInTheTick>(result.entity);
            requestsInTheTick--;
        }
    }
}

   