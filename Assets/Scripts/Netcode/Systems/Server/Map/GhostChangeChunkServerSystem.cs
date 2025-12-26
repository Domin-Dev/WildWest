using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
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
        if (query.IsEmpty) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();

        state.Dependency = new GhostChangeChunkJob()
        {
            map = mapSettings,
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
            ecb = ecb           
        }
        .ScheduleParallel(query,state.Dependency);
    }



    [BurstCompile]
    public partial struct GhostChangeChunkJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings map;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;


        public void Execute(Entity entity,in GhostChunk ghostChunk,[EntityIndexInQuery] int sortKey)
        {
            //int next = ghostChunk.GetChunk();


        }     
        
    }       


}