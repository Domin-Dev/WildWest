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



// [UpdateAfter(typeof(CalculateChunksForPlayersServerSystem))]
// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [UpdateInGroup(typeof(MapSystemGroup))]
// [RequireMatchingQueriesForUpdate]
// partial struct ChunkManagementServerSystem : ISystem
// {
//     EntityQuery playersQuery;

//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         playersQuery = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest>().Build();
//         state.RequireForUpdate<MapSettings>();
//         state.RequireForUpdate(playersQuery);
//     }

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
//         var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
//         var mapSettings = SystemAPI.GetSingleton<MapSettings>();


//         Debug.Log("--------dzial!!!" + playersQuery.CalculateEntityCount());
//         state.Dependency = new CreateChunksJob()
//         {
//             map = mapSettings,
//             loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
//             entityBuffer = SystemAPI.GetSingletonEntity<LoadedChunks>(),
//             ecb = ecb,
//             time = SystemAPI.Time.ElapsedTime,
//             maxIter = 1000     
//         }
//         .ScheduleParallel(playersQuery,state.Dependency);
//     }

//     [BurstCompile]
//     public partial struct CreateChunksJob : IJobEntity
//     {
//         public EntityCommandBuffer.ParallelWriter ecb;
//         public MapSettings map;
//         public Entity entityBuffer;
//         [ReadOnly] public double time;
//         [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

//         [ReadOnly] public int maxIter;

//         private int counter;

//         [BurstCompile]
//         public void Execute(Entity e,in LoadChunkRequest loadChunk)
//         {
//             if(counter >= maxIter) return;
//             ecb.AppendToBuffer(0,entityBuffer,new LoadedChunks()
//             {
//                 index = loadChunk.chunk,
//                 time =  time
//             });
//             ecb.DestroyEntity(0,e);
//             counter++;
//         }
//     }       
// }

   