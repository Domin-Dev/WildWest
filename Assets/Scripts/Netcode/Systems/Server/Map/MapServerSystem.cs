using System;
using System.Collections.Generic;
using System.Linq;
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
using UnityEngine;

[UpdateAfter(typeof(CollisionSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class MapServerSystem : SystemBase
{
    #region Data
    private static Map map;
    private MapGenerator generator;
    public float2 spawnPoint;
    int simulationTickRate = 60;
    private const int ChunksPerTick = 3;
    private const int renderChunksSize = 2;
    private const int maxChunkPreClient = 20;

    private NetworkTick currentTick;
    private EntitiesReferences entitiesReferences;

    private NativeHashMap<int, Entity> loadedChunks;
    private NativeList<Entity> toUnloadChunks;
    private NativeHashMap<int, NativeHashMap<int, double>> playerChunks;

    private BufferLookup<ChunkObjects> chunkObjectsLookup;

    public static Map Map { get { return map; } }
    #endregion

    protected override void OnCreate()
    {
        if (NetCodeConfig.Global != null) simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        loadedChunks = new NativeHashMap<int, Entity>(100, Allocator.Persistent);
        toUnloadChunks = new NativeList<Entity>(30, Allocator.Persistent);
        playerChunks = new NativeHashMap<int, NativeHashMap<int, double>>(50, Allocator.Persistent);
        chunkObjectsLookup = SystemAPI.GetBufferLookup<ChunkObjects>();


        NetCodeConnectionEventListener.OnClientDisconnected += OnClientDisconnected;
    }
    private void OnClientDisconnected(int NetworkId)
    {
        playerChunks[NetworkId].Dispose();
        playerChunks.Remove(NetworkId);
    }
    protected override void OnDestroy()
    {
        foreach (var item in playerChunks)
            item.Value.Dispose();
        // toUnloadChunks.Dispose();    
        loadedChunks.Dispose();
        playerChunks.Dispose();
        NetCodeConnectionEventListener.OnClientDisconnected -= OnClientDisconnected;
        base.OnDestroy();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Persistent);
        currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        ServerUnloadChunks(ref entityCommandBuffer);




        var job = new ProcessMovedEntitesJob
        {
            newChunk = SystemAPI.GetComponentTypeHandle<NewChunk>(),
            ghostChunk = SystemAPI.GetComponentTypeHandle<GhostChunk>(),
            loadedChunks = loadedChunks,
            entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
            chunkObjects = entityCommandBuffer.AsParallelWriter()
        };
        var query = SystemAPI.QueryBuilder()
                             .WithAll<GhostChunk, NewChunk>().Build();
        JobHandle jobHandle = job.ScheduleParallel(query, Dependency);
        jobHandle.Complete();


        foreach ((RefRO<SendMap> send, Entity entity) in
        SystemAPI.Query<RefRO<SendMap>>().WithEntityAccess())
        {
            if (map == null)
            {
                Debug.Log("new map!!!");
                GenerateMap();
            }

            //GetChunkObjects(0, ref entityCommandBuffer, entity);
            //GetChunkObjects(1, ref entityCommandBuffer, entity);
            //GetChunkObjects(2, ref entityCommandBuffer, entity);
            //GetChunkObjects(3, ref entityCommandBuffer, entity);


            Entity loaded = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(loaded, new MapIsLoaded() { widthInChunks = map.widthInChunks });
            entityCommandBuffer.AddComponent(loaded, new SendRpcCommandRequest()
            {
                TargetConnection = entity
            });

            entityCommandBuffer.RemoveComponent<SendMap>(entity);
        }

        foreach ((RefRO<GhostChunk> chunk, RefRO<GhostOwner> networkID, Entity entity) in
        SystemAPI.Query<RefRO<GhostChunk>, RefRO<GhostOwner>>().WithAll<NewChunk, Player>().WithEntityAccess())
        {
            var chunksToSend = map.GetNeighboringChunkIndexes(chunk.ValueRO.current, renderChunksSize);

            if (!playerChunks.ContainsKey(networkID.ValueRO.NetworkId))
                playerChunks.Add(networkID.ValueRO.NetworkId, new NativeHashMap<int, double>(maxChunkPreClient, Allocator.Persistent));

            UnloadChunks(ref entityCommandBuffer, chunksToSend, networkID.ValueRO.NetworkId);
            for (int i = 0; i < chunksToSend.Count; i++)
            {
                int index = chunksToSend[i];
                if (PlayerHasChunk(networkID.ValueRO.NetworkId, index)) continue;
                Entity chunkEntity;

                if (!loadedChunks.TryGetValue(index, out chunkEntity))
                {
                    if (!CreateChunk(ref entityCommandBuffer, index)) continue;
                    chunkEntity = loadedChunks[index];
                }

                playerChunks[networkID.ValueRO.NetworkId].Add(index, (float)SystemAPI.Time.ElapsedTime);
                entityCommandBuffer.SetComponentEnabled<NewChunkServerAction>(chunkEntity, true);
                entityCommandBuffer.AppendToBuffer(chunkEntity, new ChunkServerActions()
                {
                    networkID = networkID.ValueRO.NetworkId,
                    action = 1
                });
            }
            entityCommandBuffer.SetComponentEnabled<NewChunk>(entity, false);
        }



        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }


    #region Loadings Chunks
    public void GenerateMap()
    {
        generator = new MapGenerator(GameInfo.instance.seed);
        map = generator.StartGenerator();
    }
    private void UnloadChunks(ref EntityCommandBuffer entityCommandBuffer, List<int> neighboringChunks, int networkID)
    {
        var list = playerChunks[networkID];
        if (list.IsEmpty) return;

        var toRemove = new NativeList<int>(Allocator.Temp);
        foreach (var item in list)
        {
            if (!neighboringChunks.Contains(item.Key))
            {
                toRemove.Add(item.Key);
            }
        }

        int number = toRemove.Length + neighboringChunks.Count - maxChunkPreClient;


        while (number > 0 && toRemove.Length > 0)
        {
            int chunkIndex = GetChunkToRemove(ref toRemove, networkID);
            Entity chunk = loadedChunks[chunkIndex];

            entityCommandBuffer.AppendToBuffer(chunk, new ChunkServerActions()
            {
                networkID = networkID,
                action = 2
            });
            entityCommandBuffer.SetComponentEnabled<NewChunkServerAction>(chunk, true);

            list.Remove(chunkIndex);
            AddToUnload(chunkIndex, ref entityCommandBuffer);
            number--;
        }

        toRemove.Dispose();
    }
    private int GetChunkToRemove(ref NativeList<int> list, int networkID)
    {
        double minTime = double.MaxValue;
        int k = 0;
        for (var i = list.Length - 1; i >= 0; i--)
        {
            double time = playerChunks[networkID][list[i]];
            if (time < minTime)
            {
                k = i;
                minTime = time;
            }
        }
        int chunkIndex = list[k];
        list.RemoveAt(k);
        return chunkIndex;
    }
    private void ServerUnloadChunks(ref EntityCommandBuffer entityCommandBuffer)
    {
        if (!toUnloadChunks.IsEmpty)
        {
            for (int i = toUnloadChunks.Length - 1; i >= 0; i--)
            {
                Entity entity = toUnloadChunks[i];
                if (SystemAPI.GetBuffer<ChunkServerActions>(entity).IsEmpty)
                {
                    toUnloadChunks.RemoveAt(i);
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }
        }
    }
    private void AddToUnload(int index, ref EntityCommandBuffer entityCommandBuffer)
    {
        foreach (var item in playerChunks)
        {
            if (item.Value.ContainsKey(index))
                return;
        }
        Entity entity = loadedChunks[index];
        toUnloadChunks.Add(entity);
        loadedChunks.Remove(index);
    }
    private bool PlayerHasChunk(int networkID, int chunkIndex)
    {
        var chunks = playerChunks[networkID];
        return chunks.ContainsKey(chunkIndex);
    }
    private bool CreateChunk(ref EntityCommandBuffer entityCommandBuffer, int index)
    {
        if (!map.CheckChunkIndex(index)) return false;
        var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        Entity chunkEntity = ClientServerBootstrap.ServerWorld.EntityManager.Instantiate(entitiesReferences.chunkEntity);
        var tiles = SystemAPI.GetBuffer<ChunkTiles>(chunkEntity);
        var objects = SystemAPI.GetBuffer<BuildingObjects>(chunkEntity);
        var linkedEntitity = SystemAPI.GetBuffer<LinkedEntityGroup>(chunkEntity);


        ChunkComponent chunkComponent = new ChunkComponent();
        entityCommandBuffer.AddComponent<NewChunkServerAction>(chunkEntity);
        entityCommandBuffer.AddBuffer<ChunkServerActions>(chunkEntity);
        entityCommandBuffer.AddBuffer<ChunkObjects>(chunkEntity);

        Chunk chunk = map.chunks[index];
        chunkComponent.worldPos = chunk.worldPosition;
        chunkComponent.index = index;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GridTile tile = chunk.grid[j, i];
                tiles.Add(new ChunkTiles()
                {
                    tileID = tile.tileID,
                    variant = (byte)tile.variant
                });

                if (tile.gridObject != null)
                {
                    var obj = new BuildingObjects()
                    {
                        id = tile.gridObject.ID,
                        position = new int2(tile.x, tile.y),
                        variantIndex = tile.gridObject.variantIndex,
                        stateIndex = tile.gridObject.stateIndex,
                        hitPoints = tile.gridObject.hitPoints
                    };
                    objects.Add(obj);
                    var bObject = BuildingObjectCreator.CreateObject(ref entitiesReferences, EntityManager, ref entityCommandBuffer, obj);
                    linkedEntitity.Add(bObject);
                }
            }
        }

        entityCommandBuffer.SetComponent(chunkEntity, chunkComponent);
        loadedChunks.Add(index, chunkEntity);
        return true;
    }
    #endregion

    #region Object transfer between chunks

    [BurstCompile]
    struct ProcessMovedEntitesJob : IJobChunk
    {
        public ComponentTypeHandle<NewChunk> newChunk;
        [ReadOnly] public ComponentTypeHandle<GhostChunk> ghostChunk;
        public EntityCommandBuffer.ParallelWriter chunkObjects;
        public EntityTypeHandle entityTypeHandle;
        [ReadOnly] public NativeHashMap<int, Entity> loadedChunks;


        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var ghostChunks = chunk.GetNativeArray(ref ghostChunk);
            var entities = chunk.GetNativeArray(entityTypeHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                int current = ghostChunks[i].current;
                if (loadedChunks.TryGetValue(current, out Entity e))
                {
                    chunkObjects.AppendToBuffer(unfilteredChunkIndex,e,new ChunkObjects(){ entity = entities[i]});
                }
            }
            
            chunk.SetComponentEnabledForAll<NewChunk>(ref newChunk, false);
        }
    }
    #endregion
}




