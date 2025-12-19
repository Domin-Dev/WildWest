using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;



[UpdateAfter(typeof(ChunkManagementServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial class StartSendingChunkServerSystem : SystemBase
{
    EntityQuery requests;

    [BurstCompile]
    protected override void OnCreate()
    {
        requests = SystemAPI.QueryBuilder().WithAll<StartSendingChunkRequest,ProcessInTheTick>().Build();
        RequireForUpdate(requests);
        RequireForUpdate<MapSettings>();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged).AsParallelWriter();
        var entities = this.requests.ToEntityArray(Allocator.TempJob);
        var requestsData = this.requests.ToComponentDataArray<StartSendingChunkRequest>(Allocator.TempJob);

        Dependency = new StartSendingJob()
        {
            ecb = ecb,
            entities = entities,
            requests = requestsData,
            time = SystemAPI.Time.ElapsedTime
        }
        .Schedule(entities.Length,5,Dependency);
       

        entities.Dispose(Dependency);
        requestsData.Dispose(Dependency);
    }

    public partial struct StartSendingJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        [ReadOnly] public NativeArray<Entity> entities;
        [ReadOnly] public NativeArray<StartSendingChunkRequest> requests;
        [ReadOnly] public double time;

        public void Execute(int sortKey)
        {   
            StartSendingChunkRequest request = requests[sortKey];
            Entity e = entities[sortKey];


            ecb.AppendToBuffer(sortKey,request.playerEntity,new PlayerChunks()
            {
                chunkEntity = request.chunkEntity,
                chunkIndex = request.chunkIndex,
                time = time
            });
            ecb.AppendToBuffer(sortKey,request.chunkEntity,new PlayersNeedChunk()
            {
                playerEntity = request.playerEntity,
                networkID = request.networkID
            });

            ecb.SetComponentEnabled<NewChunkServerAction>(sortKey,request.chunkEntity, true);
            ecb.AppendToBuffer(sortKey,request.chunkEntity, new ChunkServerActions()
            {
                networkID = request.networkID,
                action = 1
            });
            ecb.DestroyEntity(sortKey,e);       
        }
    }       
}

   