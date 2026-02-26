using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[UpdateBefore(typeof(QueueRequestsServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class FindChunksToUnloadServerSystem : SystemBase
{
    EntityQuery players;
    EntityQuery chunkQuery;
    int playerCount;
    NetworkTick nextUpdate; 

    [BurstCompile]
    protected override void OnCreate()
    {
        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        chunkQuery = SystemAPI.QueryBuilder().WithAll<ChunkComponent,ChunkTimestamp,PlayersNeedChunk,ChunkObjects>().WithNone<NewChunk>().Build();
        nextUpdate = NetworkTick.Invalid;

        RequireForUpdate<MapSettings>();
        RequireForUpdate<UnloadingSettings>();
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        
    }
    
    [BurstCompile]
    protected override void OnUpdate()
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if(nextUpdate.IsValid && !currentTick.IsNewerThan(nextUpdate)) 
            return;

        playerCount = players.CalculateEntityCount();
        var settings = SystemAPI.GetSingleton<UnloadingSettings>();
        SetTimer(currentTick,settings.chunkUnloadingPeriod);
        var loadedChunk = SystemAPI.GetSingletonBuffer<LoadedChunks>();
        int limit = playerCount * settings.loadedChunksPerPlayer;

        if(loadedChunk.Length <= limit) 
            return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
        Dependency = new FindChunksToUnloadJob()
        {
            ecb = ecb.AsParallelWriter(),
            currentTime = SystemAPI.Time.ElapsedTime,
            maxIdleTime = settings.maxIdleChunkTime
        }
        .ScheduleParallel(chunkQuery,Dependency);

        Debug.Log("Unloading chunks!!!");
    }   

    [BurstCompile]
    private  void SetTimer(NetworkTick currentTick,int time)
    {
        var simulationTickRate = 60;
        if (NetCodeConfig.Global != null) simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;

        uint lifetimeInTicks = (uint)(time* simulationTickRate);
        nextUpdate = currentTick;
        nextUpdate.Add(lifetimeInTicks);
    }


    [BurstCompile]
    public partial struct FindChunksToUnloadJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public double currentTime;
        public int maxIdleTime;

        [BurstCompile]
        public void Execute(Entity chunk,in ChunkComponent chunkComponent,in ChunkTimestamp timestamp ,ref DynamicBuffer<ChunkObjects> chunkObjects, ref DynamicBuffer<PlayersNeedChunk> playersNeedChunks, [EntityIndexInQuery] int sortKey)
        {
            if(chunkObjects.IsEmpty && playersNeedChunks.IsEmpty)
            {
                if(currentTime - timestamp.timestamp > maxIdleTime)
                {
                    var entity = ecb.CreateEntity(sortKey);
                    ecb.AddComponent(sortKey,entity,new UnloadChunkRequest()
                    {
                        chunkEntity = chunk,
                        chunkIndex = chunkComponent.chunkIndex,
                    });
                }
            }
        }     
    }      
}

   