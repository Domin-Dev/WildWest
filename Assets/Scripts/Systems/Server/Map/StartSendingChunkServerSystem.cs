using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using UnityEngine;



[UpdateAfter(typeof(ChunkManagementServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class StartSendingChunkServerSystem : SystemBase
{
    EntityQuery requests;

    BufferLookup<ChunkObjects> chunkObjectsRO;
    BufferLookup<GhostChildren> childrenRO;

    [BurstCompile]
    protected override void OnCreate()
    {
        requests = SystemAPI.QueryBuilder().WithAll<StartSendingChunkRequest,ProcessInTheTick>().Build();
        chunkObjectsRO = SystemAPI.GetBufferLookup<ChunkObjects>(true);
        childrenRO = SystemAPI.GetBufferLookup<GhostChildren>(true);

        RequireForUpdate(requests);
        RequireForUpdate<MapSettings>();
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        chunkObjectsRO.Update(this);
        childrenRO.Update(this);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);
        var entities = this.requests.ToEntityArray(Allocator.TempJob);
        var requestsData = this.requests.ToComponentDataArray<StartSendingChunkRequest>(Allocator.TempJob);
        var ghostRelevancy  = SystemAPI.GetSingletonRW<GhostRelevancy>();
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
       
        foreach(var item in requestsData)
        {
            var buffer = chunkObjectsRO[item.chunkEntity];
            foreach(var chunkObj in buffer)
            {
                Debug.Log("start sending!  " + chunkObj.entity);
                var ghost = new RelevantGhostForConnection()
                {
                    Connection = item.networkID,
                    Ghost = chunkObj.ghostID
                };
     
                if(SystemAPI.HasComponent<Player>(chunkObj.entity) && !ghostRelevancy.ValueRW.GhostRelevancySet.ContainsKey(ghost))
                {
                    var owner = SystemAPI.GetComponent<GhostOwner>(chunkObj.entity);
                    var connection = SystemAPI.GetComponent<PlayerSourceConnection>(item.playerEntity);
                    Debug.Log("wyslanie gracza!!! " + owner.NetworkId + " " + connection.value);
                    RPCHelper.SendEventToClient<NewItemInHandRPC>(ecb,owner.NetworkId,tick,connection.value);       
                }
    

                ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(ghost,0);
                if(childrenRO.HasBuffer(chunkObj.entity))
                {
                    var children = childrenRO[chunkObj.entity];
                    foreach(var child in children)
                    {
                        ghost = new RelevantGhostForConnection()
                        {
                            Connection = item.networkID,
                            Ghost = child.ghostID
                        };
                        ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(ghost,0);
                    }
                }
            }
        }

        Dependency = new StartSendingJob()
        {
            ecb = ecb.AsParallelWriter(),
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

            if(request.playerEntity != Entity.Null)
            {
                ecb.SetComponent(sortKey,request.chunkEntity, new ChunkTimestamp(){ timestamp = time });
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
            }

            ecb.DestroyEntity(sortKey,e);       
        }
    }       
}

   