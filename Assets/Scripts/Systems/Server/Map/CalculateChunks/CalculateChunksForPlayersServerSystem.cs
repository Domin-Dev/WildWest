using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[UpdateAfter(typeof(CollisionSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class CalculateChunksForPlayersServerSystem : SystemBase
{
    EntityQuery playersQuery;

    private ChunkManagementServerSystem chunkManagerSystem;

    [BurstCompile]
    protected override void OnCreate()
    {
        playersQuery = SystemAPI.QueryBuilder().WithAll<GhostChunk, GhostOwner, Player, PlayerChunks, NewChunk>().Build();
        chunkManagerSystem = World.GetExistingSystemManaged<ChunkManagementServerSystem>();
       
        RequireForUpdate<MapSettings>();
        RequireForUpdate(playersQuery);
    }
    

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (playersQuery.IsEmpty) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();
        var loadedChunks =  SystemAPI.GetSingletonBuffer<LoadedChunks>();

        Dependency = new CalculateChunksForPlayersJob()
        {
            map = mapSettings,
            loadedChunks = loadedChunks,
            ecb = ecb,
            loadedChunksMap = ChunkManagementServerSystem.loadedChunks.AsReadOnly()          
        }
        .ScheduleParallel(playersQuery,Dependency);
    }
    

    [BurstCompile]
    public partial struct CalculateChunksForPlayersJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings map;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;
        [ReadOnly] public NativeParallelHashMap<int,LoadedChunks>.ReadOnly loadedChunksMap;


        [BurstCompile]
        public void Execute(Entity player,in GhostChunk ghostChunk, in GhostOwner owner, ref DynamicBuffer<PlayerChunks> playerChunks, [EntityIndexInQuery] int sortKey)
        {
            int networkID = owner.NetworkId; 
            // <int chunkindex, int priority request>
            NativeHashMap<int,int> chunksForPlayer = new NativeHashMap<int,int>(map.playerRenderCount,Allocator.TempJob);
            NativeList<(int chunk,double time)> toRemove = new NativeList<(int,double)>(map.playerRenderCount,Allocator.TempJob);

            map.GetNeighboringChunkIndexes(ghostChunk.GetChunk(),chunksForPlayer);  
            UnloadChunks(ref playerChunks, toRemove , chunksForPlayer,networkID , player, sortKey);
            
            foreach(var needChunk in chunksForPlayer)
            {
                Entity loaded = Entity.Null;
                
                if(loadedChunksMap.TryGetValue(needChunk.Key,out var loadedChunk))
                    loaded = loadedChunk.chunkEntity;
                

                Entity entity = ecb.CreateEntity(sortKey);
                if(loaded != Entity.Null)
                {
                    ecb.AddComponent(sortKey,entity, new StartSendingChunkRequest()
                    { 
                        chunkIndex = needChunk.Key,
                        chunkEntity = loaded,
                        priority = needChunk.Value, 
                        playerEntity = player,
                        networkID = networkID
                    });
                }
                else
                {
                    ecb.AddComponent(sortKey,entity, new LoadChunkRequest()
                    { 
                        chunkIndex = needChunk.Key, 
                        playerEntity = player,
                        priority = needChunk.Value,
                        networkID = networkID
                    });
                }
            }

            chunksForPlayer.Dispose();
            toRemove.Dispose();
        }     
        
        [BurstCompile]
        private void UnloadChunks(ref DynamicBuffer<PlayerChunks> playerChunks , NativeList<(int chunk,double time)> toRemove , NativeHashMap<int,int>  neighboringChunks,int networkID, Entity player,  int sortKey)
        {    
            // Remove the chunks of the set that are sent to the player.        
            int chunksToLoad = neighboringChunks.Count;
            foreach( var chunk in playerChunks)
            {
                if (neighboringChunks.ContainsKey(chunk.chunkIndex))
                {
                    neighboringChunks.Remove(chunk.chunkIndex); 
                    var entity = ecb.CreateEntity(sortKey);
                    ecb.AddComponent(sortKey,entity, new UpdateChunkTimestampRequest(){ chunkIndex = chunk.chunkIndex,chunkEntity = chunk.chunkEntity, playerEntity = player});
                }
                else
                    toRemove.Add((chunk.chunkIndex,chunk.time));
            }
            // Check the number of loaded chunks per player
            int number = toRemove.Length + chunksToLoad - map.maxChunksPerClient;
            int prio = 0;
            while (number > 0 && toRemove.Length > 0)
            {
                int chunkIndex = GetChunkToRemove(toRemove);
                var entity = ecb.CreateEntity(sortKey);
                ecb.AddComponent(sortKey,entity, new StopSendingChunkRequest(){ chunkIndex = chunkIndex,priority = prio,networkID = networkID , playerEntity = player});
                number--;
                prio++;
            }
        }
        
        [BurstCompile]
        private int GetChunkToRemove(NativeList<(int chunk,double time)> toRemove)
        {
            double minTime = toRemove[0].time;
            int k = 0;
            for (var i = toRemove.Length - 1; i >= 0; i--)
            {
                var data = toRemove[i];
                if (data.time < minTime)
                {
                    k = i;
                    minTime = data.time;
                }
            }
            int chunkIndex = toRemove[k].chunk;
            toRemove.RemoveAtSwapBack(k);
            return chunkIndex;
        } 
    }       

}