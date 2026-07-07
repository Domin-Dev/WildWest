using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GlobalRelevancySystem : ISystem
{

    private const int maxChunkEventsBufferPreClient = 50;
    private const int cutoffBorder = 100000;
    public void OnCreate(ref SystemState state)
    {
        var gh = SystemAPI.GetSingletonRW<GhostRelevancy>();
        state.RequireForUpdate<MapSettings>();


        gh.ValueRW.GhostRelevancyMode = GhostRelevancyMode.SetIsRelevant;
        NetCodeConnectionEventListener.OnClientDisconnected += OnClientDisconnected;
    }

    public void OnDestroy(ref SystemState state)
    {
        NetCodeConnectionEventListener.OnClientDisconnected -= OnClientDisconnected;
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var ghostRelevancy = SystemAPI.GetSingletonRW<GhostRelevancy>();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();

        foreach ((RefRO<GhostOwner> ghostOwner,RefRO<GhostInstance> ghost, Entity entity)
        in SystemAPI.Query<RefRO<GhostOwner>, RefRO<GhostInstance>>().WithAll<SendToOwner>().WithEntityAccess())
        {
            if(ghost.ValueRO.ghostId == 0) continue;
            var key = new RelevantGhostForConnection()
            {
                Ghost = ghost.ValueRO.ghostId,
                Connection = ghostOwner.ValueRO.NetworkId
            };
            ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(key, 0);
            entityCommandBuffer.RemoveComponent<SendToOwner>(entity);
        }


        foreach ((RefRO<ChunkComponent> chunkComponent, RefRO<GhostInstance> ghost, DynamicBuffer<ChunkServerActions> chunkRecipients,DynamicBuffer<BuildingObjects> buildingObjects, Entity entity)
        in SystemAPI.Query<RefRO<ChunkComponent>,RefRO<GhostInstance> , DynamicBuffer<ChunkServerActions>,DynamicBuffer<BuildingObjects>>().WithAll<NewChunkServerAction>().WithEntityAccess())
        {
            if (ghost.ValueRO.ghostId == 0) continue;
            
            for (int i = chunkRecipients.Length - 1; i >= 0; i--)
            {
                ChunkServerActions action = chunkRecipients[i];
                switch(action.action)
                {
                    case ServerAction.StartStreamingChunk:
                        var key = new RelevantGhostForConnection()
                        {
                            Ghost =  ghost.ValueRO.ghostId,
                            Connection = action.networkID
                        };
                        ghostRelevancy.ValueRW.GhostRelevancySet.TryAdd(key, 0);
                        StartStreamingChunks(ref state,action, ghost.ValueRO.ghostId, entity);
                        break;
                    case ServerAction.StopStreamingChunk:
                        var key2 = new RelevantGhostForConnection()
                        {
                            Ghost =  ghost.ValueRO.ghostId,
                            Connection = action.networkID
                        };
                        ghostRelevancy.ValueRW.GhostRelevancySet.Remove(key2);
                        StopStreamingChunks(ref state,action, ghost.ValueRO.ghostId, entity);
                        break;
                    case ServerAction.DamageBuildingObject:
                        if(EntityHelper.TryFindBuildingObject(action.tilePosition,buildingObjects,out var result,out int index))
                        {
                            int hp = math.clamp(result.hitPoints - action.value,0,result.maxHitPoints);
                            if(hp == 0)
                            {
                                entityCommandBuffer.DestroyEntity(result.localEntity);
                                buildingObjects.RemoveAtSwapBack(index);

                                if(ItemsAsset.instance.TryGetItem<BuildingObject>(result.id,out var itemData))
                                {
                                    float2 startPos = MyTools.ConvertFloat(mapSettings.GetEnginePositionFromTilePosition(result.globalTilePos));
                                    foreach(var drop in itemData.drop)
                                    {
                                        if(drop.probability >= UnityEngine.Random.Range(0f,1f))
                                        {
                                            int quantity = UnityEngine.Random.Range(drop.ingredient.number,drop.maxNumber + 1);
                                            for(int k = 0; k < quantity; k++)
                                            {
                                                float2 targetPos = startPos + new float2(UnityEngine.Random.Range(- 0.5f * mapSettings.tileSize,mapSettings.tileSize * 0.5f),
                                                UnityEngine.Random.Range(- 0.5f * mapSettings.tileSize,mapSettings.tileSize * 0.5f));
                                                
                                                float distance = math.distance(targetPos,startPos);
                                                EntityHelper.CreateEntityWithComponent<EQSpawnItem>(entityCommandBuffer,new EQSpawnItem(){ 

                                                    chunkIndex =  chunkComponent.ValueRO.chunkIndex,
                                                    item = new InventorySlot() { itemId = drop.ingredient.itemID, quantity = 1}, 
                                                    position = targetPos,
                                                    fromPosition = startPos,
                                                    duration = distance * 5f});
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                buildingObjects.ElementAt(index).hitPoints = hp;

                            CreateNewChunkEvent(ref state,action.networkID, new ChunkEvents()
                            {
                                chunk = chunkComponent.ValueRO.chunkIndex,
                                tilePosition = action.tilePosition,
                                flags = ChunkEventType.UpdateBuildingObject
                            });
                        }
                        break;
                }

                chunkRecipients.RemoveAt(i);
            }
            entityCommandBuffer.SetComponentEnabled<NewChunkServerAction>(entity,false);

        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void StartStreamingChunks(ref SystemState state,ChunkServerActions action,int ghostID, Entity entity)
    {
        CreateNewChunkEvent(ref state,action.networkID, new ChunkEvents()
        {
            chunk = SystemAPI.GetComponent<ChunkComponent>(entity).chunkIndex,
            flags = ChunkEventType.LoadChunk
        });
    }
    private void CreateNewChunkEvent(ref SystemState state, int networkID, ChunkEvents chunkEvent)
    {
        foreach ((DynamicBuffer<ChunkEvents> events, RefRO<GhostOwner> ghostOwner, RefRW<ServerChunkEventCounter> counter, RefRO<ChunkEventCounter> clientCounter)
        in SystemAPI.Query<DynamicBuffer<ChunkEvents>, RefRO<GhostOwner>, RefRW<ServerChunkEventCounter>, RefRO<ChunkEventCounter>>().WithAll<Player>())
        {
            if (ghostOwner.ValueRO.NetworkId == networkID)
            {
                chunkEvent.index = counter.ValueRO.index;
                events.Add(chunkEvent);
                counter.ValueRW.index++;
                if(events.Length > maxChunkEventsBufferPreClient)
                {
                    for (int i = events.Length - 1; i >= 0; i--)
                    {
                        long dis = Math.Abs((long)events[i].index - (long)clientCounter.ValueRO.index);
                        if ((events[i].index < clientCounter.ValueRO.index && dis < cutoffBorder)
                          ||(events[i].index > clientCounter.ValueRO.index && dis > cutoffBorder))
                        {
                            events.RemoveAt(i);
                        }
                    }

                }
            }
        }
    }
    private void StopStreamingChunks(ref SystemState state, ChunkServerActions action, int ghostID, Entity entity)
    {
        CreateNewChunkEvent(ref state, action.networkID, new ChunkEvents()
        {
            chunk = SystemAPI.GetComponent<ChunkComponent>(entity).chunkIndex,
            flags = ChunkEventType.UnloadChunk
        });
    }

    public static void OnClientDisconnected(int connectionId)
    {
        // var keysToRemove = new NativeList<RelevantGhostForConnection>(Allocator.Temp);

        // foreach (var kvp in ghostRelevancy.GhostRelevancySet)
        // {
        //     if (kvp.Key.Connection == connectionId)
        //     {
        //         keysToRemove.Add(kvp.Key);
        //     }
        // }
        // foreach (var key in keysToRemove)
        // {
        //     ghostRelevancy.GhostRelevancySet.Remove(key);
        // }
        // keysToRemove.Dispose();
    }
    public static void OnGhostDestroyed(int ghostID)
    {
        // var keysToRemove = new NativeList<RelevantGhostForConnection>(Allocator.Temp);
        // foreach (var kvp in ghostRelevancy.GhostRelevancySet)
        // {
        //     if (kvp.Key.Ghost == ghostID)
        //     {
        //         keysToRemove.Add(kvp.Key);
        //     }
        // }
        // foreach (var key in keysToRemove)
        // {
        //     ghostRelevancy.GhostRelevancySet.Remove(key);
        // }
        // keysToRemove.Dispose();
    }

}
