using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;



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


    public static NativeParallelHashMap<int,LoadedChunks> loadedChunks;

    protected override void OnCreate()
    {
        LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest,ProcessInTheTick>().Build();
        loadedChunks = new NativeParallelHashMap<int, LoadedChunks>(10,Allocator.Persistent);

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
        loadedChunks.Dispose();
    }

    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();
        var entities = LoadRequests.ToEntityArray(Allocator.TempJob);
        var requests = LoadRequests.ToComponentDataArray<LoadChunkRequest>(Allocator.TempJob);


        var job = new CreateChunksJob()
        {
            mapSettings = mapSettings,
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
            entityBuffer = SystemAPI.GetSingletonEntity<LoadedChunks>(),
            ecb = ecb,
            time = SystemAPI.Time.ElapsedTime,  
            entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>(),
            entities = entities,
            requests = requests,
        }
        .Schedule(entities.Length,3,Dependency);
        job.Complete();


        entities.Dispose();
        requests.Dispose();
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
            if(mapSettings.CheckChunkIndex(loadChunk.chunkIndex))
            {
     
                CreateChunk(loadChunk.chunkIndex,sortKey,out Entity entity);
                var loadedChunk = new LoadedChunks()
                {
                    chunkIndex = loadChunk.chunkIndex,
                    chunkEntity = entity
                };
                ecb.AppendToBuffer(sortKey,entityBuffer,loadedChunk);
                if(loadChunk.playerEntity != Entity.Null)
                {
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
                        action = ServerAction.StartStreamingChunk
                    });
                }
            }
            ecb.DestroyEntity(sortKey,e);       
        }

        [BurstCompile]
        private bool CreateChunk(int index,int sortKey, out Entity entity)
        {
            entity = Entity.Null;
            Entity chunkEntity = ecb.Instantiate(sortKey,entitiesReferences.chunkEntity);     
            ecb.AddComponent<ToSave>(sortKey,chunkEntity);
            ecb.SetComponentEnabled<ToSave>(sortKey,chunkEntity,true);

            ecb.AddComponent<NewChunkServerAction>(sortKey,chunkEntity);
            ecb.AddBuffer<ChunkServerActions>(sortKey,chunkEntity); 
            ecb.AddBuffer<ChunkObjects>(sortKey,chunkEntity);
            ecb.AddBuffer<PlayersNeedChunk>(sortKey,chunkEntity);


            ecb.AddComponent(sortKey,chunkEntity, new ChunkTimestamp(){ timestamp = time });

            NativeArray<ChunkTiles> chunkTiles = new NativeArray<ChunkTiles>(mapSettings.tilesCount,Allocator.Temp);
            NativeList<BuildingObjects> buildingObjects = new NativeList<BuildingObjects>(mapSettings.tilesCount,Allocator.Temp);

            ChunkComponent chunkComponent = MapGenerator.GenerateRegion(index,in mapSettings, chunkTiles, buildingObjects);
            foreach(ChunkTiles tile in chunkTiles)
            {
                ecb.AppendToBuffer(sortKey,chunkEntity,tile);
            }

            for (int i = 0; i < buildingObjects.Length;i++)
            {      
                var bObject = buildingObjects[i];
                Entity obj = BuildingObjectCreator.CreateObjectServer(ref ecb,bObject,sortKey);
                bObject.localEntity = obj;
                ecb.AppendToBuffer(sortKey,chunkEntity,bObject);
                ecb.AppendToBuffer<LinkedEntityGroup>(sortKey,chunkEntity,obj);
            }
            
            GoInGameServerSystem.CreateNewContainer(chunkEntity,ecb,index,ref entitiesReferences,new ContainerStats()
            {
                capacity = 1000,
                containerIndex = EquipmentConfig.chunkItems_ContainerIndex,
                serverContainer = true,
                publicContainer = true,
                waterResistance = 0,
                mandatoryProperties = MandatoryProperties.none,
                mandatoryData = 0,
            },null,-1,sortKey);


            ecb.SetComponent(sortKey,chunkEntity, chunkComponent);
            entity = chunkEntity;
            chunkTiles.Dispose();
            buildingObjects.Dispose();
            return true;
        }
    }       
}

   