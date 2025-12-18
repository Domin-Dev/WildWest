using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;



[UpdateAfter(typeof(QueueRequestsServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial class ChunkManagementServerSystem : SystemBase
{
    EntityQuery LoadRequests;

    public static ServerMap Map { get { return map; } }
    private static ServerMap map;
    private MapGenerator generator;

    protected override void OnCreate()
    {
        LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest,ProcessInTheTick>().Build();
   
        RequireForUpdate(LoadRequests);
        RequireForUpdate<MapSettings>();

        if(SystemAPI.TryGetSingleton(out MapSettings mapSettings))
        {
            generator = new MapGenerator(mapSettings.seed);
            if(mapSettings.newMap) generator.GenerateMap(ref map,mapSettings);
        }
        else
            Enabled = false;
    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        map.Dispose();
    }

    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();
        var entities = LoadRequests.ToEntityArray(Allocator.TempJob);
        var requests = LoadRequests.ToComponentDataArray<LoadChunkRequest>(Allocator.TempJob);


        Dependency = new CreateChunksJob()
        {
            mapSettings = mapSettings,
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
            entityBuffer = SystemAPI.GetSingletonEntity<LoadedChunks>(),
            ecb = ecb,
            time = SystemAPI.Time.ElapsedTime,  
            entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>(),
            entities = entities,
            requests = requests
          //  map = map.chunks.AsReadOnly()         
        }
        .Schedule(entities.Length,3,Dependency);

        entities.Dispose(Dependency);
        requests.Dispose(Dependency);
     }


    public void GenerateMap()
    {
       // generator = new MapGenerator(GameInfo.instance.seed);
       // generator.StartGenerator(ref map);
    }

    public void LoadMap()
    {
       // generator = new MapGenerator(GameInfo.instance.seed);
       // map = generator.StartGenerator();
    }


    public partial struct CreateChunksJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings mapSettings;
        public EntitiesReferences entitiesReferences;
        public Entity entityBuffer;

        
        [ReadOnly] public NativeArray<Entity> entities;
        [ReadOnly] public NativeArray<LoadChunkRequest> requests;


        [ReadOnly] public double time;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

        public void Execute(int sortKey)
        {   
            LoadChunkRequest loadChunk = requests[sortKey];
            Entity e = entities[sortKey];


            CreateChunk(loadChunk.chunkIndex,sortKey ,out Entity entity);


            ecb.AppendToBuffer(sortKey,entityBuffer,new LoadedChunks()
            {
                chunkIndex = loadChunk.chunkIndex,
                time =  time,
                chunkEntity = entity
            });
            ecb.AppendToBuffer(sortKey,loadChunk.playerEntity,new PlayerChunks()
            {
                chunkEntity = entity,
                chunkIndex = loadChunk.chunkIndex,
                time = time
            });
            ecb.AppendToBuffer(sortKey,entity,new PlayersNeedChunk()
            {
                playerEntity = loadChunk.playerEntity,
                networkID = loadChunk.networkID
            });
            ecb.SetComponentEnabled<NewChunkServerAction>(sortKey,entity, true);
            ecb.AppendToBuffer(sortKey,entity, new ChunkServerActions()
            {
                networkID = loadChunk.networkID,
                action = 1
            });
            ecb.DestroyEntity(sortKey,e);       
        }

        [BurstCompile]
        private bool CreateChunk(int index,int sortKey, out Entity entity)
        {
            entity = Entity.Null;
           // if (!map.CheckChunkIndex(index)) return false;

            Entity chunkEntity = ecb.Instantiate(sortKey,entitiesReferences.chunkEntity);          
            ecb.AddComponent<NewChunkServerAction>(sortKey,chunkEntity);
            ecb.AddBuffer<ChunkServerActions>(sortKey,chunkEntity);
           
            ecb.AddBuffer<ChunkObjects>(sortKey,chunkEntity);
            ecb.AddBuffer<PlayersNeedChunk>(sortKey,chunkEntity);

            NativeArray<ChunkTiles> chunkTiles = new NativeArray<ChunkTiles>(mapSettings.tilesCount,Allocator.Temp);

            ChunkComponent chunkComponent = MapGenerator.GenerateRegion(index,in mapSettings, chunkTiles);
            foreach(ChunkTiles tile in chunkTiles)
            {
                ecb.AppendToBuffer(sortKey,chunkEntity,tile);

                // if (tile != null)
                // {
                //     var obj = new BuildingObjects()
                //     {
                //         id = tile.gridObject.ID,
                //         position = new int2(tile.x, tile.y),
                //         variantIndex = tile.gridObject.variantIndex,
                //         stateIndex = tile.gridObject.stateIndex,
                //         hitPoints = tile.gridObject.hitPoints
                //     };
                //     ecb.AppendToBuffer(unfilteredChunkIndex,chunkEntity,obj);
                //     ecb.AppendToBuffer<LinkedEntityGroup>(unfilteredChunkIndex,chunkEntity,BuildingObjectCreator.CreateObjectServer(entitiesReferences,ref ecb, obj,unfilteredChunkIndex));
                // }
            }

            ecb.SetComponent(sortKey,chunkEntity, chunkComponent);
            entity = chunkEntity;

            chunkTiles.Dispose();
            return true;
        }
    }       
}

   