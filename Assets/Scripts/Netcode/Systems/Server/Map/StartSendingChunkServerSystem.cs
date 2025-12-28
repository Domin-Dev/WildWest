using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;



[UpdateAfter(typeof(ChunkManagementServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class StartSendingChunkServerSystem : SystemBase
{
    EntityQuery requests;

    BufferLookup<ChunkObjects> chunkObjectsRO;

    [BurstCompile]
    protected override void OnCreate()
    {
        requests = SystemAPI.QueryBuilder().WithAll<StartSendingChunkRequest,ProcessInTheTick>().Build();
        chunkObjectsRO = SystemAPI.GetBufferLookup<ChunkObjects>(true);


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
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged).AsParallelWriter();
        var entities = this.requests.ToEntityArray(Allocator.TempJob);
        var requestsData = this.requests.ToComponentDataArray<StartSendingChunkRequest>(Allocator.TempJob);
        var ghostRelevancy  = SystemAPI.GetSingletonRW<GhostRelevancy>();

        Dependency = new StartSendingJob()
        {
            ecb = ecb,
            entities = entities,
            requests = requestsData,
            time = SystemAPI.Time.ElapsedTime
        }
        .Schedule(entities.Length,5,Dependency);
       
        foreach(var item in requestsData)
        {
            var buffer = chunkObjectsRO[item.chunkEntity];
            foreach(var element in buffer)
            {
                var ghost = new RelevantGhostForConnection()
                {
                    Connection = item.networkID,
                    Ghost = element.ghostID
                };
                ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(ghost,0);
            }
        }


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
            ecb.DestroyEntity(sortKey,e);       
        }
    }       
}

   