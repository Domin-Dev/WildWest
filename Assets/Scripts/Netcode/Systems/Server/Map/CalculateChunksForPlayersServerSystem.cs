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
partial struct CalculateChunksForPlayersServerSystem : ISystem
{
    EntityQuery playersQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        playersQuery = SystemAPI.QueryBuilder().WithAll<GhostChunk, GhostOwner, Player, PlayerChunks, NewChunk>().Build();
        state.RequireForUpdate<MapSettings>();
        state.RequireForUpdate(playersQuery);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (playersQuery.IsEmpty) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();

        state.Dependency = new CalculateChunksForPlayersJob()
        {
            map = mapSettings,
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
            ecb = ecb           
        }
        .ScheduleParallel(playersQuery,state.Dependency);
    }
    [BurstCompile]
    public partial struct CalculateChunksForPlayersJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings map;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

        [BurstCompile]
        public void Execute(Entity player,in GhostChunk ghostChunk, in GhostOwner owner, ref DynamicBuffer<PlayerChunks> playerChunks, [EntityIndexInQuery] int sortKey)
        {
            int networkID = owner.NetworkId; 
            NativeHashSet<int> chunksForPlayer = new NativeHashSet<int>(map.playerRenderCount,Allocator.TempJob);
            NativeList<(int chunk,double time)> toRemove = new NativeList<(int,double)>(map.playerRenderCount,Allocator.TempJob);

            map.GetNeighboringChunkIndexes(ghostChunk.current,chunksForPlayer);
            UnloadChunks(ref playerChunks, toRemove , chunksForPlayer, player, sortKey);
            foreach(int index in chunksForPlayer)
            {
                bool loaded =  false;
                foreach(var chunk in loadedChunks)
                {
                    if(index == chunk.index)
                    {
                        loaded = true;
                        break;
                    }                
                } 
                Entity entity = ecb.CreateEntity(0);
                if(loaded)
                    ecb.AddComponent(sortKey,entity, new StartSendingChunkRequest(){ chunk = index, player = player});
                else
                {
                    ecb.AddComponent(sortKey,entity, new LoadChunkRequest(){ chunk = index, player = player , priority = index + 99 , networkID = networkID});
                }
            }
           ecb.SetComponentEnabled<NewChunk>(0,player,false);
           chunksForPlayer.Dispose();
           toRemove.Dispose();
        }     
        
        [BurstCompile]
        private void UnloadChunks(ref DynamicBuffer<PlayerChunks> playerChunks , NativeList<(int chunk,double time)> toRemove , NativeHashSet<int> neighboringChunks, Entity player,  int sortKey)
        {    
            // Remove the chunks of the set that are sent to the player.        
            int chunksToLoad = neighboringChunks.Count;
            foreach( var chunk in playerChunks)
            {
                if (neighboringChunks.Contains(chunk.index))
                    neighboringChunks.Remove(chunk.index); 
                else
                    toRemove.Add((chunk.index,chunk.time));
            }
            // Check the number of loaded chunks per player
            int number = toRemove.Length + chunksToLoad - map.maxChunksPerClient;
            while (number > 0 && toRemove.Length > 0)
            {
                int chunkIndex = GetChunkToRemove(toRemove);
                var entity = ecb.CreateEntity(sortKey);
                ecb.AddComponent(sortKey,entity, new StopSendingChunkRequest(){ chunk = chunkIndex, player = player});
                number--;
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