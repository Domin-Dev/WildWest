using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;



[UpdateAfter(typeof(ChunkManagementServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class UpdateTimestampServerSystem : SystemBase
{
    EntityQuery requests;
    EntityQuery players;


    int playerCount;
    BufferLookup<PlayerChunks> playerChunksRW;

    // <Entity player,int chunkIndex> 
    NativeParallelMultiHashMap<Entity,int> chunksToUpdate;

    [BurstCompile]
    protected override void OnCreate()
    {
        requests = SystemAPI.QueryBuilder().WithAll<UpdateChunkTimestampRequest>().Build();
        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        playerChunksRW = SystemAPI.GetBufferLookup<PlayerChunks>();
        
        chunksToUpdate = new NativeParallelMultiHashMap<Entity, int>(256,Allocator.Persistent);

        RequireForUpdate(requests);
        RequireForUpdate<TickLimitsConfig>();
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        chunksToUpdate.Dispose();
    }
    
    [BurstCompile]
    protected override void OnUpdate()
    {
        playerChunksRW.Update(this);

        var tickLimits = SystemAPI.GetSingleton<TickLimitsConfig>(); 
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);
      

        playerCount = players.CalculateEntityCount();

        double time = SystemAPI.Time.ElapsedTime;

        int requestsInTheTick = Math.Min(playerCount * tickLimits.updateChunkTimeRequestsInTickPerClient,tickLimits.maxUpdateChunkTimeRequestsInTick);
        foreach ((RefRO<UpdateChunkTimestampRequest> requestData, Entity entity) in SystemAPI.Query<RefRO<UpdateChunkTimestampRequest>>().WithEntityAccess())
        {
            if(requestsInTheTick == 0) break;
            chunksToUpdate.Add(requestData.ValueRO.playerEntity,requestData.ValueRO.chunkIndex);
            ecb.SetComponent(requestData.ValueRO.chunkEntity, new ChunkTimestamp(){timestamp = time});
            ecb.DestroyEntity(entity);
            requestsInTheTick--;
        }

        var keys = chunksToUpdate.GetKeyArray(Allocator.Temp);
        foreach(Entity player in keys)
        {
            var values = chunksToUpdate.GetValuesForKey(player);
            var buffer = playerChunksRW[player];
            foreach(int value in values)
            {
                int index = 0;
                foreach(var item in buffer)
                {
                    if(item.chunkIndex == value)
                    {
                        buffer.ElementAt(index).time = time;
                    }
                    index++;
                }
            }            
            values.Dispose();
        }
        keys.Dispose();
        chunksToUpdate.Clear();
    }     
}

   