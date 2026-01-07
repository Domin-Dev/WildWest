using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEditor.Tilemaps;
using UnityEngine;




[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup),OrderLast = true)]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class SavingChunksServerSystem : SystemBase
{

    EntityQuery chunkQuery;
    NetworkTick nextUpdate;

    public static  NativeList<(int region,ChunkData chunk)> chunksToSaveRW;    
    public static  NativeList<(int region,ChunkData chunk)> chunksToSaveRO;    

    [BurstCompile]
    protected override void OnCreate()
    {
        chunkQuery = SystemAPI.QueryBuilder().WithAll<ChunkComponent,BuildingObjects,ChunkTiles>().WithAll<ChunkIsDirty>().WithNone<NewChunk>().Build();
        nextUpdate = NetworkTick.Invalid;
        chunksToSaveRW = new NativeList<(int,ChunkData)>(128,Allocator.Persistent);
        chunksToSaveRO = new NativeList<(int,ChunkData)>(128,Allocator.Persistent);

        RequireForUpdate<SavesConfig>();

        if(SystemAPI.TryGetSingleton<MapSettings>(out var map))
            ChunkSaveIOThread.Start(map.chunksCountInRegion);
    }


    [BurstCompile]
    protected override void OnDestroy()
    {
        ChunkSaveIOThread.Stop();
        chunksToSaveRO.Dispose();
        chunksToSaveRW.Dispose();
    }



    [BurstCompile]
    protected override void OnUpdate()
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if(nextUpdate.IsValid && !currentTick.IsNewerThan(nextUpdate)) 
            return;


        var settings = SystemAPI.GetSingleton<SavesConfig>();
        SetTimer(currentTick,settings.savePeriod);
        if(chunkQuery.IsEmpty)
        {
            Swap();
            return;
        }


        var map = SystemAPI.GetSingleton<MapSettings>();
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        foreach ((RefRO<ChunkComponent> chunkComp,DynamicBuffer<ChunkTiles> tiles,DynamicBuffer<BuildingObjects> buildingObjects, Entity entity) in SystemAPI.Query<RefRO<ChunkComponent>,DynamicBuffer<ChunkTiles>,DynamicBuffer<BuildingObjects>>().WithAll<ChunkIsDirty>().WithNone<NewChunk>().WithEntityAccess())
        {
            NativeArray<TileData> tileDatas = new NativeArray<TileData>(tiles.Length,Allocator.Persistent);
            NativeArray<BuildingObjectData> buildingObjs = new NativeArray<BuildingObjectData>(buildingObjects.Length,Allocator.Persistent);
            for (int i = 0; i < tiles.Length; i++)
                tileDatas[i] = new TileData(tiles[i]);

            for (int i = 0; i < buildingObjects.Length; i++)
                buildingObjs[i] = new BuildingObjectData(buildingObjects[i]);  
            

            chunksToSaveRW.Add((chunkComp.ValueRO.regionIndex,new ChunkData()
            {
                chunkIndex = chunkComp.ValueRO.chunkIndex,
                localChunkIndex = map.GetRegionChunkIndex(chunkComp.ValueRO.chunkIndex),
                tiles = tileDatas,
                objects = buildingObjs
            }));
            ecb.SetComponentEnabled<ChunkIsDirty>(entity,false);
        }

        Swap();
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
    private void Swap()
    {
        if(chunksToSaveRO.IsEmpty && chunksToSaveRW.Length > 0)
        {
            var tmp = chunksToSaveRW;
            chunksToSaveRW = chunksToSaveRO;
            chunksToSaveRO = tmp;
            ChunkSaveIOThread.Notify();
        }
    }
}

   