using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;



//[UpdateAfter(typeof(QueueRequestsServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial class ChunkManagementServerSystem : SystemBase
{
    EntityQuery LoadRequests;
    EntityQuery players;

    public static ServerMap Map { get { return map; } }
    private static ServerMap map;
    private MapGenerator generator;


    protected override void OnCreate()
    {
        LoadRequests = SystemAPI.QueryBuilder().WithAll<LoadChunkRequest,ProcessInTheTick>().Build();
        players = SystemAPI.QueryBuilder().WithAll<Player>().Build();

        RequireForUpdate(LoadRequests);
        RequireForUpdate<MapSettings>();
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
    
        Debug.Log("dzialakok!!!!!!!!!!!  " + LoadRequests.CalculateEntityCount() );

        Dependency = new CreateChunksJob()
        {
            mapSettings = mapSettings,
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(),
            entityBuffer = SystemAPI.GetSingletonEntity<LoadedChunks>(),
            ecb = ecb,
            time = SystemAPI.Time.ElapsedTime,  
            entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>(),
            map = map.chunks         
        }
        .ScheduleParallel(LoadRequests,Dependency);
    }


    public void GenerateMap()
    {
        generator = new MapGenerator(GameInfo.instance.seed);
        generator.StartGenerator(ref map);
    }

    public void LoadMap()
    {
       // generator = new MapGenerator(GameInfo.instance.seed);
       // map = generator.StartGenerator();
    }



    [BurstCompile]
    public partial struct CreateChunksJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings mapSettings;
        public EntitiesReferences entitiesReferences;
        public Entity entityBuffer;
        public NativeHashMap<int,ServerChunk> map;


        [ReadOnly] public double time;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;


        [BurstCompile]
        public void Execute(Entity e,in LoadChunkRequest loadChunk, [EntityIndexInQuery] int sortKey)
        {
            ecb.AppendToBuffer(sortKey,entityBuffer,new LoadedChunks()
            {
                index = loadChunk.chunk,
                time =  time
            });


            CreateChunk(loadChunk.chunk,sortKey ,out Entity entity);
            ecb.SetComponentEnabled<NewChunkServerAction>(sortKey,entity, true);
            ecb.AppendToBuffer(sortKey,entity, new ChunkServerActions()
            {
                networkID = loadChunk.networkID,
                action = 1
            });
            ecb.DestroyEntity(sortKey,e);       
        }

        private bool CreateChunk(int index,int sortKey, out Entity entity)
        {
            entity = Entity.Null;
           // if (!map.CheckChunkIndex(index)) return false;

            Entity chunkEntity = ecb.Instantiate(sortKey,entitiesReferences.chunkEntity);
            ChunkComponent chunkComponent = new ChunkComponent();
            ecb.AddComponent<NewChunkServerAction>(sortKey,chunkEntity);
            ecb.AddBuffer<ChunkServerActions>(sortKey,chunkEntity);
            ecb.AddBuffer<ChunkObjects>(sortKey,chunkEntity);
            ecb.AddBuffer<PlayersNeedChunk>(sortKey,chunkEntity);
            ecb.AddBuffer<ChunkObjects>(sortKey,chunkEntity);

            ServerChunk chunk = map[index];
            chunkComponent.worldPos = chunk.worldPosition;
            chunkComponent.index = index;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    ServerTile tile = chunk.grid[j, i];
                    ecb.AppendToBuffer(sortKey,chunkEntity,new ChunkTiles()
                    {
                        tileID = tile.tileID,
                        variant = (byte)tile.variant
                    });

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
            }

            ecb.SetComponent(sortKey,chunkEntity, chunkComponent);
            entity = chunkEntity;
            return true;
        }
    }       
}

   