using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
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
    private static int renderChunksCount => (2 * renderChunksSize + 1)*(2 * renderChunksSize + 1);

    private const int maxChunkPerClient = 30;

    private NetworkTick currentTick;


    private NativeHashMap<int, Entity> loadedChunks;
    private NativeList<Entity> toUnloadChunks;

    private NativeParallelMultiHashMap<int,int> playerChunks;
    private NativeParallelHashMap<(int networkID,int chunkIndex),double> times;


    private NativeQueue<(Entity e,int chunkIndex)> objectsToRemoveFromBuffer;



    // Changes
    public NativeParallelMultiHashMap<int,int> stopSending;
    public NativeParallelMultiHashMap<int,(int chunkIndex, Entity chunk)> startSending;

    public static Map Map { get { return map; } }
    #endregion

    protected override void OnCreate()
    {
        if (NetCodeConfig.Global != null) simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        loadedChunks = new NativeHashMap<int, Entity>(100, Allocator.Persistent);
        toUnloadChunks = new NativeList<Entity>(30, Allocator.Persistent);
        playerChunks = new NativeParallelMultiHashMap<int,int>(100, Allocator.Persistent);
        times = new NativeParallelHashMap<(int networkID,int chunkIndex),double>(100,Allocator.Persistent);
        objectsToRemoveFromBuffer = new NativeQueue<(Entity,int)>(Allocator.Persistent); 

        stopSending = new NativeParallelMultiHashMap<int,int>(10,Allocator.Persistent);
        startSending = new NativeParallelMultiHashMap<int,(int chunkIndex, Entity chunk)>(10,Allocator.Persistent);

        

        RequireForUpdate<EntitiesReferences>();

        NetCodeConnectionEventListener.OnClientDisconnected += OnClientDisconnected;
    }
    private void OnClientDisconnected(int NetworkId)
    {
        playerChunks.Remove(NetworkId);
    }
    protected override void OnDestroy()
    {
        playerChunks.Dispose();    
        toUnloadChunks.Dispose();    
        loadedChunks.Dispose();
        objectsToRemoveFromBuffer.Dispose();

        startSending.Dispose();
        stopSending.Dispose();
        times.Dispose();

        NetCodeConnectionEventListener.OnClientDisconnected -= OnClientDisconnected;
        base.OnDestroy();
    }
    protected override void OnUpdate()
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var entityCommandBuffer = ecbSystem.CreateCommandBuffer(EntityManager.WorldUnmanaged);
        EntityCommandBuffer.ParallelWriter ecb = entityCommandBuffer.AsParallelWriter();


    //     currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
    //   //  ServerUnloadChunks(ref entityCommandBuffer);
    //     // objectsToRemoveFromBuffer.Clear();
    //     // stopSending.Clear();
    //     // startSending.Clear();



    //     // var job1 = new LoadChunksJob
    //     // {
    //     //     loadedChunks = loadedChunks,
    //     //     ghostChunk = SystemAPI.GetComponentTypeHandle<GhostChunk>(true),
    //     //     ghostOwner = SystemAPI.GetComponentTypeHandle<GhostOwner>(true),
    //     //     entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
    //     //     ecb = ecb,
    //     //     playerChunks = playerChunks,
    //     //     times = times,
    //     //     startSending = startSending.AsParallelWriter(),
    //     //     stopSending = stopSending.AsParallelWriter(),
    //     //     entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>()
    //     // };
    //     // JobHandle jobHandle1 = job1.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<GhostChunk, NewChunk, GhostOwner, Player>().Build(), Dependency);
    //     // jobHandle1.Complete();


    //     // var job = new ProcessMovedEntitesJob
    //     // {
    //     //     newChunk = SystemAPI.GetComponentTypeHandle<NewChunk>(),
    //     //     ghostChunk = SystemAPI.GetComponentTypeHandle<GhostChunk>(true),
    //     //     loadedChunks = loadedChunks,
    //     //     entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
    //     //     ecb = ecb,
    //     //     objectsToRemoveFromBuffer = objectsToRemoveFromBuffer.AsParallelWriter()
    //     // };
    //     // var jobHandle2 = job.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<GhostChunk, NewChunk>().Build(), jobHandle1);
    //     // jobHandle2.Complete();startSending

    //     LoadChanges();
        
    //     Debug.Log("dzila!!!! " + loadedChunks.Count);

        foreach ((RefRO<SendMap> send, Entity entity) in
        SystemAPI.Query<RefRO<SendMap>>().WithEntityAccess())
        {
            if (map == null)
            {
                GenerateMap();
            }

            //GetChunkObjects(0, ref entityCommandBuffer, entity);
            //GetChunkObjects(1, ref entityCommandBuffer, entity);
            //GetChunkObjects(2, ref entityCommandBuffer, entity);
            //GetChunkObjects(3, ref entityCommandBuffer, entity);


            Entity loaded = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(loaded, new MapIsLoaded() { widthInChunks = 100 });
            entityCommandBuffer.AddComponent(loaded, new SendRpcCommandRequest()
            {
                TargetConnection = entity
            });

            entityCommandBuffer.RemoveComponent<SendMap>(entity);
        }


    }

    private void LoadChanges()
    {
        foreach(var keyValue in startSending)
        {
            // start sending chunk to the player
            playerChunks.Add(keyValue.Key,keyValue.Value.chunkIndex);
            times.Add((keyValue.Key,keyValue.Value.chunkIndex),SystemAPI.Time.ElapsedTime);

            if(!loadedChunks.ContainsKey(keyValue.Value.chunkIndex))
            {
                loadedChunks.Add(keyValue.Value.chunkIndex,keyValue.Value.chunk);    
                Debug.Log("new chunk! " + keyValue.Value.chunk);
            }
        }

        // var keys = stopSending.GetKeyArray(Allocator.Temp);
        // foreach(var key in keys)
        // {
        //     var values = stopSending.GetValuesForKey(key);
        //     var sending = playerChunks.GetValuesForKey(key);

        //     foreach (var value in values)
        //     {
        //         foreach( var chunk in sending)
        //         {
        //             if(value == chunk.chunk)
        //             {
        //                 playerChunks.Remove<
        //             }
        //         }   
        //     }

        //     values.Dispose();
        // }

        // keys.Dispose();
    }

   // #region Loadings Chunks
    public void GenerateMap()
    {
       // generator = new MapGenerator(GameInfo.instance.seed);
        //map = generator.StartGenerator();
    }
    

    // private void ServerUnloadChunks(ref EntityCommandBuffer entityCommandBuffer)
    // {
    //     if (!toUnloadChunks.IsEmpty)
    //     {
    //         for (int i = toUnloadChunks.Length - 1; i >= 0; i--)
    //         {
    //             Entity entity = toUnloadChunks[i];
    //             if (SystemAPI.GetBuffer<ChunkServerActions>(entity).IsEmpty)
    //             {
    //                 toUnloadChunks.RemoveAt(i);
    //                 entityCommandBuffer.DestroyEntity(entity);
    //             }
    //         }
    //     }
    // }
    


    // #endregion



        // private void AddToUnload(int index)
        // {
        //     foreach (var item in playerChunks)
        //     {
        //         if (item.Value.ContainsKey(index))
        //             return;
        //     }
        //     Entity entity = loadedChunks[index];
        //     toUnloadChunks.Add(entity);
        //     loadedChunks.Remove(index);
        // }
    #region Object transfer between chunks

    struct ProcessMovedEntitesJob : IJobChunk
    {
        public ComponentTypeHandle<NewChunk> newChunk;
        [ReadOnly] public ComponentTypeHandle<GhostChunk> ghostChunk;
        public EntityCommandBuffer.ParallelWriter ecb;
        public EntityTypeHandle entityTypeHandle;
        [ReadOnly] public NativeHashMap<int, Entity> loadedChunks;
        public NativeQueue<(Entity,int)>.ParallelWriter objectsToRemoveFromBuffer;


        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var ghostChunks = chunk.GetNativeArray(ref ghostChunk);
            var entities = chunk.GetNativeArray(entityTypeHandle);
        
        
            for (int i = 0; i < chunk.Count; i++)
            {
                var gchunk = ghostChunks[i];
                var entity = entities[i];
                if (loadedChunks.TryGetValue(gchunk.current, out Entity e))
                    ecb.AppendToBuffer(unfilteredChunkIndex,e,new ChunkObjects(){ entity = entity});

                if(gchunk.LastChunkIsNotNull())
                    objectsToRemoveFromBuffer.Enqueue((entity,gchunk.lastChunk));
            }
              
            chunk.SetComponentEnabledForAll(ref newChunk, false);
        }

    }

    struct LoadChunksJob : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<GhostChunk> ghostChunk;
        [ReadOnly] public ComponentTypeHandle<GhostOwner> ghostOwner;

        [ReadOnly] public EntitiesReferences entitiesReferences;

        [ReadOnly] public NativeHashMap<int, Entity> loadedChunks;
        [ReadOnly] public NativeParallelMultiHashMap<int,int> playerChunks;
        [ReadOnly] public NativeParallelHashMap<(int networkID,int chunkIndex),double> times;

        public NativeParallelMultiHashMap<int,int>.ParallelWriter stopSending;
        public NativeParallelMultiHashMap<int,(int chunkIndex, Entity chunk)>.ParallelWriter startSending;



        public EntityCommandBuffer.ParallelWriter ecb;
        public EntityTypeHandle entityTypeHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var ghostChunks = chunk.GetNativeArray(ref ghostChunk);
            var ghostOwners = chunk.GetNativeArray(ref ghostOwner);
            var entities = chunk.GetNativeArray(entityTypeHandle);


            NativeHashSet<int> ChunkToSend = new NativeHashSet<int>(renderChunksCount,Allocator.TempJob);
            NativeList<(int chunk,double time)> toRemove = new NativeList<(int,double)>(renderChunksCount,Allocator.TempJob);
            

            for(int i = 0; i < chunk.Count; i++)
            {
                int networkID = ghostOwners[i].NetworkId;
                var gChunk = ghostChunks[i]; 
                ChunkToSend.Clear();
                toRemove.Clear();

                Map.GetNeighboringChunkIndexes(gChunk.current, renderChunksSize,ChunkToSend);
                UnloadChunks(toRemove,ChunkToSend,networkID,unfilteredChunkIndex);

                foreach(int index  in ChunkToSend)
                {
                    Entity chunkEntity;
                    if (!loadedChunks.TryGetValue(index, out chunkEntity) && !CreateChunk(networkID,index,unfilteredChunkIndex, out chunkEntity)) 
                        continue;
                    
                    startSending.Add(networkID,(index,chunkEntity));
                    ecb.SetComponentEnabled<NewChunkServerAction>(unfilteredChunkIndex,chunkEntity, true);
                    ecb.AppendToBuffer(unfilteredChunkIndex,chunkEntity, new ChunkServerActions()
                    {
                        networkID = networkID,
                        action = 1
                    });
                }
            }
            toRemove.Dispose();
            ChunkToSend.Dispose();
        }   
        private void UnloadChunks(NativeList<(int chunk,double time)> toRemove , NativeHashSet<int> neighboringChunks, int networkID, int unfilteredChunkIndex)
        {    
            // Remove the chunks of the set that are sent to the player.        
            int chunksToLoad = neighboringChunks.Count;
            if (playerChunks.TryGetFirstValue(networkID, out var value, out var iterator))
            {
                if (!neighboringChunks.Contains(value))
                    toRemove.Add((value,times[(networkID,value)]));
                else
                    neighboringChunks.Remove(value);
                
                while (playerChunks.TryGetNextValue(out value, ref iterator))
                {
                    if (!neighboringChunks.Contains(value))
                        toRemove.Add((value,times[(networkID,value)]));
                    else
                        neighboringChunks.Remove(value);
                }
            }
   
            // Check the number of loaded chunks per player
            int number = toRemove.Length + chunksToLoad - maxChunkPerClient;

            while (number > 0 && toRemove.Length > 0)
            {
                int chunkIndex = GetChunkToRemove(toRemove);
                Entity chunk = loadedChunks[chunkIndex];
                // ecb.AppendToBuffer(unfilteredChunkIndex,chunk, new ChunkServerActions()
                // {
                //     networkID = networkID,
                //     action = 2
                // });
               // ecb.SetComponentEnabled<NewChunkServerAction>(unfilteredChunkIndex,chunk, true);
                stopSending.Add(networkID,chunkIndex);
                number--;
            }
        }
        private int GetChunkToRemove(NativeList<(int chunk,double time)> toRemove)
        {
            double minTime = toRemove[0].time;
            int k = 0;
            for (var i = toRemove.Length - 1; i >= 0; i--)
            {
                var data = toRemove[i];
                if (data.time < minTime)
                {
                    k = i;
                    minTime = data.time;
                }
            }
            int chunkIndex = toRemove[k].chunk;
            toRemove.RemoveAtSwapBack(k);
            return chunkIndex;
        } 
        private bool CreateChunk(int networkID,int index,int unfilteredChunkIndex, out Entity entity)
        {
            entity = Entity.Null;
            if (!map.CheckChunkIndex(index)) return false;

            Entity chunkEntity = ecb.Instantiate(unfilteredChunkIndex,entitiesReferences.chunkEntity);
            ChunkComponent chunkComponent = new ChunkComponent();
            ecb.AddComponent<NewChunkServerAction>(unfilteredChunkIndex,chunkEntity);
            ecb.AddBuffer<ChunkServerActions>(unfilteredChunkIndex,chunkEntity);
            ecb.AddBuffer<ChunkObjects>(unfilteredChunkIndex,chunkEntity);
            ecb.AddBuffer<PlayersNeedChunk>(unfilteredChunkIndex,chunkEntity);

            Chunk chunk = map.chunks[index];
            chunkComponent.worldPos = chunk.worldPosition;
            chunkComponent.index = index;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    GridTile tile = chunk.grid[j, i];
                    ecb.AppendToBuffer(unfilteredChunkIndex,chunkEntity,new ChunkTiles()
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
                        ecb.AppendToBuffer(unfilteredChunkIndex,chunkEntity,obj);
                        ecb.AppendToBuffer<LinkedEntityGroup>(unfilteredChunkIndex,chunkEntity,BuildingObjectCreator.CreateObjectServer(entitiesReferences,ref ecb, obj,unfilteredChunkIndex));
                    }
                }
            }

            ecb.SetComponent(unfilteredChunkIndex,chunkEntity, chunkComponent);
            entity = chunkEntity;
            return true;
        }
    }
    #endregion
}






 