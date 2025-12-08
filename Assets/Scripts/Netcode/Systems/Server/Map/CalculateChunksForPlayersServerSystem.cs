using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;




[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
partial struct CalculateChunksForPlayersServerSystem : ISystem
{
    EntityQuery playersQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        playersQuery = SystemAPI.QueryBuilder().WithAll<GhostChunk, NewChunk, GhostOwner, Player, PlayerChunks>().Build();
        state.RequireForUpdate<MapSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
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
        public void Execute(Entity player,in GhostChunk ghostChunk, in GhostOwner owner, ref DynamicBuffer<PlayerChunks> playerChunks)
        {
            // int networkID = owner.NetworkId; 
            // NativeHashSet<int> chunksForPlayer = new NativeHashSet<int>(map.playerRenderCount,Allocator.TempJob);
            // NativeList<(int chunk,double time)> toRemove = new NativeList<(int,double)>(map.playerRenderCount,Allocator.TempJob);
            // map.GetNeighboringChunkIndexes(ghostChunk.current,chunksForPlayer);
            // UnloadChunks(ref playerChunks, toRemove , chunksForPlayer, player);
            // foreach(int index in chunksForPlayer)
            // {
            //     bool loaded =  false;
            //     foreach(var chunk in loadedChunks)
            //     {
            //         if(index == chunk.index)
            //         {
            //             loaded = true;
            //             break;
            //         }                
            //     } 
                var entity = ecb.CreateEntity(0);
             //   if(loaded)
              //      ecb.AddComponent(0,entity, new StartSendingChunkRequest(){ chunk = index, player = player});
            //    else
                    ecb.AddComponent(0,entity, new LoadChunkRequest(){ chunk = 10, player = player });
            //}

         //   ecb.SetComponentEnabled<NewChunk>(0,player,false);
        //    chunksForPlayer.Dispose();
        //    toRemove.Dispose();
        }     
        
        [BurstCompile]
        private void UnloadChunks(ref DynamicBuffer<PlayerChunks> playerChunks , NativeList<(int chunk,double time)> toRemove , NativeHashSet<int> neighboringChunks, Entity player)
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
                var entity = ecb.CreateEntity(0);
                ecb.AddComponent(0,entity, new StopSendingChunkRequest(){ chunk = chunkIndex, player = player});
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

   