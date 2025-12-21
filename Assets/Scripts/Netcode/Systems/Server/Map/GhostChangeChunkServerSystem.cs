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
partial struct GhostChangeChunkServerSystem : ISystem
{
    EntityQuery query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        query = SystemAPI.QueryBuilder().WithAll<GhostChunk, NewChunk>().Build();
        state.RequireForUpdate<MapSettings>();
        state.RequireForUpdate(query);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // if (query.IsEmpty) return;
        // var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        // var mapSettings = SystemAPI.GetSingleton<MapSettings>();

        // state.Dependency = new CalculateChunksForPlayersJob()
        // {
        //     map = mapSettings,
        //     loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
        //     ecb = ecb           
        // }
        // .ScheduleParallel(query,state.Dependency);
    }
    //[BurstCompile]
    // public partial struct CalculateChunksForPlayersJob : IJobEntity
    // {
    //     public EntityCommandBuffer.ParallelWriter ecb;
    //     public MapSettings map;
    //     [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

    //     [BurstCompile]
    //     public void Execute(Entity player,in GhostChunk ghostChunk, in GhostOwner owner, ref DynamicBuffer<PlayerChunks> playerChunks, [EntityIndexInQuery] int sortKey)
    //     {
    //         int networkID = owner.NetworkId; 
    //         NativeHashMap<int,int> chunksForPlayer = new NativeHashMap<int,int>(map.playerRenderCount,Allocator.TempJob);
    //         NativeList<(int chunk,double time)> toRemove = new NativeList<(int,double)>(map.playerRenderCount,Allocator.TempJob);

    //         map.GetNeighboringChunkIndexes(ghostChunk.current,chunksForPlayer);
    //         UnloadChunks(ref playerChunks, toRemove , chunksForPlayer,networkID , player, sortKey);
            
    //         foreach(var needChunk in chunksForPlayer)
    //         {
    //             bool loaded =  false;

    //             foreach(var chunk in loadedChunks)
    //             {
    //                 if(needChunk.Key == chunk.chunkIndex)
    //                 {
    //                     loaded = true;
    //                     break;
    //                 }                
    //             } 
    //             Entity entity = ecb.CreateEntity(sortKey);
    //             if(loaded)
    //                 ecb.AddComponent(sortKey,entity, new StartSendingChunkRequest(){ chunkIndex = needChunk.Key, playerEntity = player});
    //             else
    //             {
    //                 ecb.AddComponent(sortKey,entity, new LoadChunkRequest(){ chunkIndex = needChunk.Key, playerEntity = player , priority = needChunk.Value , networkID = networkID});
    //             }
    //         }
    //         Entity chunkChange = ecb.CreateEntity(sortKey);

    //         ecb.SetComponentEnabled<NewChunk>(0,player,false);
    //         chunksForPlayer.Dispose();
    //         toRemove.Dispose();
    //     }     
        
    //     [BurstCompile]
    //     private void UnloadChunks(ref DynamicBuffer<PlayerChunks> playerChunks , NativeList<(int chunk,double time)> toRemove , NativeHashMap<int,int>  neighboringChunks,int networkID, Entity player,  int sortKey)
    //     {    
    //         // Remove the chunks of the set that are sent to the player.        
    //         int chunksToLoad = neighboringChunks.Count;
    //         foreach( var chunk in playerChunks)
    //         {
    //             if (neighboringChunks.ContainsKey(chunk.chunkIndex))
    //                 neighboringChunks.Remove(chunk.chunkIndex); 
    //             else
    //                 toRemove.Add((chunk.chunkIndex,chunk.time));
    //         }
    //         // Check the number of loaded chunks per player
    //         int number = toRemove.Length + chunksToLoad - map.maxChunksPerClient;
    //         int prio = 0;
    //         Debug.Log(number + "   to remove" );
    //         while (number > 0 && toRemove.Length > 0)
    //         {
    //             int chunkIndex = GetChunkToRemove(toRemove);
    //             var entity = ecb.CreateEntity(sortKey);
    //             ecb.AddComponent(sortKey,entity, new StopSendingChunkRequest(){ chunk = chunkIndex,priority = prio,networkID = networkID , player = player});
    //             number--;
    //             prio++;
    //         }
    //     }
        
    //     [BurstCompile]
    //     private int GetChunkToRemove(NativeList<(int chunk,double time)> toRemove)
    //     {
    //         double minTime = toRemove[0].time;
    //         int k = 0;
    //         for (var i = toRemove.Length - 1; i >= 0; i--)
    //         {
    //             var data = toRemove[i];
    //             if (data.time < minTime)
    //             {
    //                 k = i;
    //                 minTime = data.time;
    //             }
    //         }
    //         int chunkIndex = toRemove[k].chunk;
    //         toRemove.RemoveAtSwapBack(k);
    //         return chunkIndex;
    //     } 
    // }       


}