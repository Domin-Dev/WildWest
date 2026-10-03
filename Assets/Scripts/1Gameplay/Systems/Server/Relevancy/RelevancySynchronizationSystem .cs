using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct RelevancySynchronizationSystem : ISystem
{

    BufferLookup<PlayersNeedChunk> needChunkRO;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GhostRelevancy>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<GhostInstance,SynchronizeRelevancyWithParent>().WithAll<Simulate>().Build());
       
        needChunkRO = SystemAPI.GetBufferLookup<PlayersNeedChunk>(true);
    }


    public void OnUpdate(ref SystemState state)
    {
        needChunkRO.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var ghostRelevancy = SystemAPI.GetSingletonRW<GhostRelevancy>();

        foreach ((RefRO<GhostInstance> ghostInstance ,RefRO<SynchronizeRelevancyWithParent> sync,Entity entity) in
        SystemAPI.Query<RefRO<GhostInstance> ,RefRO<SynchronizeRelevancyWithParent>>().WithAll<Simulate>().WithEntityAccess())
        {
            if(ghostInstance.ValueRO.ghostId == 0) continue;

            if(SystemAPI.HasComponent<GhostChunk>(sync.ValueRO.parent))
            {
                int chunkIndex = SystemAPI.GetComponent<GhostChunk>(sync.ValueRO.parent).GetChunk();
                if(ChunkManagementServerSystem.loadedChunks.TryGetValue(chunkIndex,out var chunk))
                {
                    var players = needChunkRO[chunk.chunkEntity];
                    foreach(var item in players)
                    {
                        RelevantGhostForConnection connection = new RelevantGhostForConnection()
                        {
                            Connection = item.networkID,
                            Ghost = ghostInstance.ValueRO.ghostId 
                        };       
                        ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(connection,0);
                    }
                }
            }
            entityCommandBuffer.RemoveComponent<SynchronizeRelevancyWithParent>(entity);
        }
    }
}
