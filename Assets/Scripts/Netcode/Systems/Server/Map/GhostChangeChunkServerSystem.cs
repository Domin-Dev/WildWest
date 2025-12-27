using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;



[UpdateAfter(typeof(CollisionSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class GhostChangeChunkServerSystem : SystemBase
{
    EntityQuery query;

    ChunkManagementServerSystem chunkManagerSystem;

    private NativeQueue<(Entity Chunk,Entity entity)> entitiesToRemove;
    
    private BufferLookup<ChunkObjects> chunkObjects;


    [BurstCompile] 
    protected override void OnCreate()
    {
        query = SystemAPI.QueryBuilder().WithAll<GhostChunk, NewChunk>().Build();
        chunkManagerSystem = World.GetExistingSystemManaged<ChunkManagementServerSystem>();
        entitiesToRemove = new NativeQueue<(Entity Chunk,Entity entity)>(Allocator.Persistent);
        chunkObjects = SystemAPI.GetBufferLookup<ChunkObjects>();

        RequireForUpdate<MapSettings>();
        RequireForUpdate(query);
    }
    
    [BurstCompile] 
    protected override void OnDestroy()
    {
        entitiesToRemove.Dispose();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        if (query.IsEmpty) return;

        chunkObjects.Update(this);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();

        var job = new GhostChangeChunkJob()
        {
            map = mapSettings,
            loadedChunks = chunkManagerSystem.loadedChunks.AsReadOnly(),
            ecb = ecb,
            entitiesToRemove = entitiesToRemove.AsParallelWriter()          
        }
        .ScheduleParallel(query,Dependency);
        job.Complete();

        while(entitiesToRemove.TryDequeue(out var pair))
        {
            var buffer = chunkObjects[pair.Chunk];
            for(int i = 0; i < buffer.Length; i++)
            {
                if(buffer[i].entity == pair.entity)
                {
                    buffer.RemoveAtSwapBack(i);
                    break;
                }
            }
        }
    }



    [BurstCompile]
    public partial struct GhostChangeChunkJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings map;
        [ReadOnly] public NativeParallelHashMap<int,LoadedChunks>.ReadOnly loadedChunks;

        public NativeQueue<(Entity Chunk,Entity entity)>.ParallelWriter entitiesToRemove;

        public void Execute(Entity entity,in GhostChunk ghostChunk,[EntityIndexInQuery] int sortKey)
        {       
            int current = ghostChunk.GetChunk();
            int last = ghostChunk.GetLastChunk();

            if(current >= 0)
            {
                if(loadedChunks.TryGetValue(current,out var chunk))
                {
                    ecb.AppendToBuffer(sortKey,chunk.chunkEntity, new ChunkObjects(entity)); 
                    ecb.SetComponentEnabled<NewChunk>(sortKey,entity,false);
                }
            }

            if(last >= 0)
            {
                if(loadedChunks.TryGetValue(last,out var chunk))
                    entitiesToRemove.Enqueue((chunk.chunkEntity,entity));
            }
            ecb.SetComponent(sortKey,entity,ghostChunk);
        }     
        
    }      


}