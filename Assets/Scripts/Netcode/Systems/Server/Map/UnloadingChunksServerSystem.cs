using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;




[UpdateAfter(typeof(ChunkManagementServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class UnloadingChunksServerSystem : SystemBase
{

    EntityQuery requests;
    [BurstCompile]
    protected override void OnCreate()
    {
        requests = SystemAPI.QueryBuilder().WithAll<UnloadChunkRequest,ProcessInTheTick>().Build();
        RequireForUpdate(requests);
        RequireForUpdate<LoadedChunks>();
    }

    [BurstCompile]
    protected override void OnDestroy()
    {

    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);
        var loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>();

        foreach ((RefRO<UnloadChunkRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<UnloadChunkRequest>>().WithAll<ProcessInTheTick>().WithEntityAccess())
        {
            Debug.Log("remove!!" + requestData.ValueRO.chunkIndex);
            
            if(SystemAPI.Exists(requestData.ValueRO.chunkEntity))
                ecb.DestroyEntity(requestData.ValueRO.chunkEntity);

            for(int i = 0; i < loadedChunks.Length; i++)
            {
                if(loadedChunks[i].chunkIndex == requestData.ValueRO.chunkIndex)
                {
                    loadedChunks.RemoveAtSwapBack(i);
                    break;
                }
            }
            ecb.DestroyEntity(entity);
        }


    }
}

   