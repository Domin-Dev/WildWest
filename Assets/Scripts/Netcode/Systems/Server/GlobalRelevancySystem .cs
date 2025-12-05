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
    static GhostRelevancy ghostRelevancy;

    private const int maxChunkEventsBufferPreClient = 50;
    private const int cutoffBorder = 100000;
    public void OnCreate(ref SystemState state)
    {
        var gh = SystemAPI.GetSingletonRW<GhostRelevancy>();
        gh.ValueRW.GhostRelevancyMode = GhostRelevancyMode.SetIsRelevant;
        ghostRelevancy = gh.ValueRO;
        NetCodeConnectionEventListener.OnClientDisconnected += OnClientDisconnected;
    }

    public void OnDestroy(ref SystemState state)
    {
        NetCodeConnectionEventListener.OnClientDisconnected -= OnClientDisconnected;
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);


        foreach ((RefRO<InterestArea> area, RefRO<LocalTransform> playerPos, RefRO<GhostOwner> ghostOwner, Entity entity)
        in SystemAPI.Query<RefRO<InterestArea>, RefRO<LocalTransform>, RefRO<GhostOwner>>().WithEntityAccess())
        {
            foreach ((RefRO<GhostInstance> ghost, RefRO<LocalTransform> ghostPos, Entity ghostObj)
            in SystemAPI.Query<RefRO<GhostInstance>,RefRO<LocalTransform>>().WithAll<Physics2D>().WithEntityAccess())
            {
                if(entity == ghostObj) continue;
                bool isRelevant = math.distance(MyTools.ConvertFloat(playerPos.ValueRO.Position), MyTools.ConvertFloat(ghostPos.ValueRO.Position)) < area.ValueRO.radius;
                var key = new RelevantGhostForConnection()
                {
                    Ghost = ghost.ValueRO.ghostId,
                    Connection = ghostOwner.ValueRO.NetworkId
                };

                if (isRelevant)
                    ghostRelevancy.GhostRelevancySet.TryAdd(key, 0); 
                else if(ghostRelevancy.GhostRelevancySet.TryGetValue(key,out int item))
                    ghostRelevancy.GhostRelevancySet.Remove(key);

            }
        }



        foreach ((RefRO<GhostOwner> ghostOwner,RefRO<GhostInstance> ghost, Entity entity)
        in SystemAPI.Query<RefRO<GhostOwner>, RefRO<GhostInstance>>().WithAll<SendToOwner>().WithEntityAccess())
        {
            if(ghost.ValueRO.ghostId == 0) continue;
            var key = new RelevantGhostForConnection()
            {
                Ghost = ghost.ValueRO.ghostId,
                Connection = ghostOwner.ValueRO.NetworkId
            };
            ghostRelevancy.GhostRelevancySet.Add(key, 0);
            entityCommandBuffer.RemoveComponent<SendToOwner>(entity);
        }


        foreach ((RefRO<GhostInstance> ghost, DynamicBuffer<ChunkServerActions> chunkRecipients, Entity entity)
        in SystemAPI.Query<RefRO<GhostInstance> , DynamicBuffer<ChunkServerActions>>().WithAll<NewChunkServerAction>().WithEntityAccess())
        {
            if (ghost.ValueRO.ghostId == 0) continue;
            
            for (int i = chunkRecipients.Length - 1; i >= 0; i--)
            {
                ChunkServerActions action = chunkRecipients[i];
                switch(action.action)
                {
                    case 1:
                        StartStreamingChunks(ref state,action, ghost.ValueRO.ghostId, entity);
                        break;
                    case 2:
                        StopStreamingChunks(ref state,action, ghost.ValueRO.ghostId, entity);
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
        var key = new RelevantGhostForConnection()
        {
            Ghost = ghostID,
            Connection = action.networkID
        };
        if(!ghostRelevancy.GhostRelevancySet.ContainsKey(key))
            ghostRelevancy.GhostRelevancySet.Add(key, 0);
        CreateNewChunkEvent(ref state,action.networkID, new ChunkEvents()
        {
            value = SystemAPI.GetComponent<ChunkComponent>(entity).index,
            flags = 1
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
        var key = new RelevantGhostForConnection()
        {
            Ghost = ghostID,
            Connection = action.networkID
        };
        ghostRelevancy.GhostRelevancySet.Remove(key);
        CreateNewChunkEvent(ref state, action.networkID, new ChunkEvents()
        {
            value = SystemAPI.GetComponent<ChunkComponent>(entity).index,
            flags = 2
        });
    }

    public static void OnClientDisconnected(int connectionId)
    {
        var keysToRemove = new NativeList<RelevantGhostForConnection>(Allocator.Temp);

        foreach (var kvp in ghostRelevancy.GhostRelevancySet)
        {
            if (kvp.Key.Connection == connectionId)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            ghostRelevancy.GhostRelevancySet.Remove(key);
        }
        keysToRemove.Dispose();
    }
    public static void OnGhostDestroyed(int ghostID)
    {
        var keysToRemove = new NativeList<RelevantGhostForConnection>(Allocator.Temp);
        foreach (var kvp in ghostRelevancy.GhostRelevancySet)
        {
            if (kvp.Key.Ghost == ghostID)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            ghostRelevancy.GhostRelevancySet.Remove(key);
        }
        keysToRemove.Dispose();
    }

}
