using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;




[UpdateAfter(typeof(ChunkManagementServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class StopSendingChunkServerSystem : SystemBase
{
    EntityQuery requests;
    BufferLookup<PlayersNeedChunk> needsChunks;
    BufferLookup<PlayerChunks> playerChunks;

    NativeQueue<(Entity chunk,StopSendingChunkRequest request)> toRemove;
    [BurstCompile]
    protected override void OnCreate()
    {
        requests = SystemAPI.QueryBuilder().WithAll<StopSendingChunkRequest,ProcessInTheTick>().Build();
        toRemove = new NativeQueue<(Entity chunk, StopSendingChunkRequest request)>(Allocator.Persistent);

        RequireForUpdate(requests);
        RequireForUpdate<MapSettings>();

        needsChunks = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
        playerChunks = SystemAPI.GetBufferLookup<PlayerChunks>();
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        toRemove.Dispose();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        needsChunks.Update(this);
        playerChunks.Update(this);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged).AsParallelWriter();
        var entities = this.requests.ToEntityArray(Allocator.TempJob);
        var requestsData = this.requests.ToComponentDataArray<StopSendingChunkRequest>(Allocator.TempJob);

        var job = new StopSendingJob()
        {
            ecb = ecb,
            toRemove = this.toRemove.AsParallelWriter(),
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
            entities = entities,
            requests = requestsData
        }
        .Schedule(entities.Length,3,Dependency);
       
        job.Complete();
        while(toRemove.TryDequeue(out var item))
        {
            var buffer = needsChunks[item.chunk];
            for(int i =0; i < buffer.Length;i++)
            {
                if(buffer[i].networkID == item.request.networkID)
                {
                    buffer.RemoveAtSwapBack(i);
                    break;
                }
            }

            var chunks = playerChunks[item.request.player];
            for(int i =0; i < chunks.Length;i++)
            {
                if(chunks[i].chunkIndex == item.request.chunk)
                {
                    chunks.RemoveAtSwapBack(i);
                    break;
                }
            }
        }
        entities.Dispose(Dependency);
        requestsData.Dispose(Dependency);
     }


    [BurstCompile]
    public partial struct StopSendingJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public NativeQueue<(Entity chunk, StopSendingChunkRequest request)>.ParallelWriter toRemove;

        [ReadOnly] public NativeArray<Entity> entities;
        [ReadOnly] public NativeArray<StopSendingChunkRequest> requests;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

        [BurstCompile]
        public void Execute(int sortKey)
        {   
            StopSendingChunkRequest request = requests[sortKey];
            Entity e = entities[sortKey];
            LoadedChunks loadedChunk = new LoadedChunks();

            foreach(var chunk in loadedChunks)
            {
                if(chunk.chunkIndex == request.chunk)
                {
                    loadedChunk = chunk;
                }
            }
            if(loadedChunk.chunkEntity != Entity.Null)
            {
                ecb.SetComponentEnabled<NewChunkServerAction>(sortKey,loadedChunk.chunkEntity, true);
                ecb.AppendToBuffer(sortKey,loadedChunk.chunkEntity, new ChunkServerActions()
                {
                    networkID = request.networkID,
                    action = 2
                });
                toRemove.Enqueue((loadedChunk.chunkEntity,request));
            }
            ecb.DestroyEntity(sortKey,e);       
        }

    }       
}

   