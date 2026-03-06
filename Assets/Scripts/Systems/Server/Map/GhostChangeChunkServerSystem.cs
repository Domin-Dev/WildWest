using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;



[UpdateAfter(typeof(CollisionSystem))]
[UpdateBefore(typeof(GhostSendSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class GhostChangeChunkServerSystem : SystemBase
{
    EntityQuery query;

    ChunkManagementServerSystem chunkManagerSystem;

    private NativeQueue<(Entity chunk,Entity entity)> entitiesToRemove;
    private NativeQueue<(Entity chunk, int ghostID, Entity entity)> sendGhostsToPlayers;



    private BufferLookup<ChunkObjects> chunkObjects;
    private BufferLookup<PlayersNeedChunk> playersNeedChunk;
    private BufferLookup<GhostChildren> childrenRO;

    [BurstCompile] 
    protected override void OnCreate()
    {
        query = SystemAPI.QueryBuilder().WithAll<GhostChunk,GhostInstance, NewChunk>().Build();
        chunkManagerSystem = World.GetExistingSystemManaged<ChunkManagementServerSystem>();
        entitiesToRemove = new NativeQueue<(Entity chunk,Entity entity)>(Allocator.Persistent);
        sendGhostsToPlayers = new  NativeQueue<(Entity chunk, int ghostID,Entity entity)>(Allocator.Persistent);


        chunkObjects = SystemAPI.GetBufferLookup<ChunkObjects>();
        playersNeedChunk = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
        childrenRO = SystemAPI.GetBufferLookup<GhostChildren>(true);

        RequireForUpdate<MapSettings>();
        RequireForUpdate(query);
    }
    
    [BurstCompile] 
    protected override void OnDestroy()
    {
        entitiesToRemove.Dispose();
        sendGhostsToPlayers.Dispose();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        if (query.IsEmpty) return;

        chunkObjects.Update(this);
        playersNeedChunk.Update(this);
        childrenRO.Update(this);

        var ghostRelevancy  = SystemAPI.GetSingletonRW<GhostRelevancy>();


        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();

        var job = new GhostChangeChunkJob()
        {
            map = mapSettings,
            loadedChunks = ChunkManagementServerSystem.loadedChunks.AsReadOnly(),
            ecb = ecb,
            entitiesToRemove = entitiesToRemove.AsParallelWriter(),
            sendGhostsToPlayers = sendGhostsToPlayers.AsParallelWriter()        
        }
        .ScheduleParallel(query,Dependency);
        job.Complete();


        while(entitiesToRemove.TryDequeue(out var pair))
        {
            var buffer = chunkObjects[pair.chunk];
            int ghostID = -1;
            bool removed = false;

            for(int i = 0; i < buffer.Length; i++)
            {
                if(buffer[i].entity == pair.entity)
                {
                    ghostID = buffer[i].ghostID;
                    buffer.RemoveAtSwapBack(i);
                    removed = true;
                    break;
                }
            }

            if(removed)
            {
                var players = playersNeedChunk[pair.chunk];
                foreach (var player in players)
                {
                    if(player.playerEntity == pair.entity) continue;
                    var element =  new RelevantGhostForConnection()
                    {
                        Connection = player.networkID,
                        Ghost = ghostID
                    };
                    if(ghostRelevancy.ValueRW.GhostRelevancySet.ContainsKey(element))
                        ghostRelevancy.ValueRW.GhostRelevancySet.Remove(element);


                    
                    if(childrenRO.HasBuffer(pair.entity))
                    {
                        var children = childrenRO[pair.entity];
                        foreach(var child in children)
                        {
                            element = new RelevantGhostForConnection()
                            {
                                Connection = player.networkID,
                                Ghost = child.ghostID
                            };
                            Debug.Log("stop!!! " + element.Connection + "  " + element.Ghost);

                            if(ghostRelevancy.ValueRW.GhostRelevancySet.ContainsKey(element))
                                ghostRelevancy.ValueRW.GhostRelevancySet.Remove(element);
                        }
                    }

                } 
            }

        }
    
        while(sendGhostsToPlayers.TryDequeue(out var pair))
        {
            var players = playersNeedChunk[pair.chunk];
            foreach (var player in players)
            {
                var element =  new RelevantGhostForConnection()
                {
                    Connection = player.networkID,
                    Ghost = pair.ghostID
                };
                ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(element,0);

                if(childrenRO.HasBuffer(pair.entity))
                {
                    var children = childrenRO[pair.entity];
                    foreach(var child in children)
                    {
                        element = new RelevantGhostForConnection()
                        {
                            Connection = player.networkID,
                            Ghost = child.ghostID
                        };
                        ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(element,0);
                    }
                }
            } 
        }
    }

    public static void RemoveGhost()
    {
        
    }

    [BurstCompile]
    public partial struct GhostChangeChunkJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings map;
        [ReadOnly] public NativeParallelHashMap<int,LoadedChunks>.ReadOnly loadedChunks;
        

        

        public NativeQueue<(Entity chunk, int ghostID, Entity entity)>.ParallelWriter sendGhostsToPlayers;
        public NativeQueue<(Entity chunk,Entity entity)>.ParallelWriter entitiesToRemove;

        public void Execute(Entity entity,in GhostInstance ghostInstance,GhostChunk ghostChunk,[EntityIndexInQuery] int sortKey)
        {    
            int current = ghostChunk.GetChunk();
            int last = ghostChunk.GetLastChunk();
            int ghostID = ghostInstance.ghostId;

            if(current >= 0)
            {
                if(loadedChunks.TryGetValue(current,out var chunk))
                {
                    ecb.AppendToBuffer(sortKey,chunk.chunkEntity, new ChunkObjects(entity,ghostID)); 
                    ecb.SetComponentEnabled<NewChunk>(sortKey,entity,false);
                    sendGhostsToPlayers.Enqueue((chunk.chunkEntity,ghostID,entity));
                    ghostChunk.SetChunkEntity(chunk.chunkEntity);
                }
            }
            else
            {
                ghostChunk.SetChunkEntity(Entity.Null);
            }

            if(last >= 0)
            {
                if(loadedChunks.TryGetValue(last,out var chunk))
                    entitiesToRemove.Enqueue((chunk.chunkEntity,entity));
            }
            ecb.SetComponent(sortKey,entity,ghostChunk);
        }     
        
    }      


}