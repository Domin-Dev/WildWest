using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using UnityEngine.XR;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[RequireMatchingQueriesForUpdate]
partial struct WorldItemsClientSystem : ISystem
{
    private BufferLookup<WorldItems> worldItemsLookup;
    private ComponentLookup<LocalTransform> transformLookup;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<ClientChunks>();

        transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        worldItemsLookup = SystemAPI.GetBufferLookup<WorldItems>(true);

        state.RequireForUpdate<VisualEffectsBuffer>();
    }



    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        transformLookup.Update(ref state);
        worldItemsLookup.Update(ref state);

        state.CompleteDependency();
        var currentTime = SystemAPI.GetSingleton<NetworkTime>();
        var prefabs = SystemAPI.GetSingleton<EntitiesReferences>();
        var chunks = SystemAPI.GetSingleton<ClientChunks>();
        

        foreach ((RefRO<MergeItemsPRC> action,Entity rpc) in SystemAPI.Query<RefRO<MergeItemsPRC>>().WithEntityAccess())
        {    
            if(currentTime.InterpolationTick.IsNewerThan(action.ValueRO.tick))
            {
                Entity chunkFrom;
                Entity chunkTo;

                WorldItems from = new WorldItems();
                WorldItems to = new WorldItems();

                if(action.ValueRO.fromChunkIndex == action.ValueRO.toChunkIndex)
                {
                    chunkFrom = chunkTo = chunks.currentChunks[action.ValueRO.toChunkIndex];
                    var worldItems = worldItemsLookup[chunkFrom];
                    foreach(var item in worldItems)
                    {
                        if(action.ValueRO.fromSlotIndex == item.slot)
                            from = item;
                        else if(action.ValueRO.toSlotIndex == item.slot)
                            to = item;
                    }
                }
                else
                {
                    chunkTo = chunks.currentChunks[action.ValueRO.toChunkIndex];
                    chunkFrom = chunks.currentChunks[action.ValueRO.fromChunkIndex];

                    var worldItems = worldItemsLookup[chunkFrom];
                    foreach(var item in worldItems)
                    {
                        if(action.ValueRO.fromSlotIndex == item.slot)
                            from = item;
                    }

                    worldItems = worldItemsLookup[chunkTo];
                    foreach(var item in worldItems)
                    {
                        if(action.ValueRO.toSlotIndex == item.slot)
                            to = item;
                    }
                }

                if(transformLookup.TryGetComponent(from.worldItem,out var transform))
                {
                    entityCommandBuffer.AddComponent<MoveToTarget>(from.worldItem,new MoveToTarget()
                    {
                        duration = action.ValueRO.duration,
                        startTick = action.ValueRO.tick,
                        targetEntity = to.worldItem,
                        destroy = true,
                        startPosition = new float2(transform.Position.x,transform.Position.y)
                    });
                }        
                entityCommandBuffer.DestroyEntity(rpc);
            }
            else if(SystemAPI.HasComponent<ReceiveRpcCommandRequest>(rpc))
            {
                var command = SystemAPI.GetComponent<ReceiveRpcCommandRequest>(rpc);
                if(!command.IsConsumed)
                {
                    command.Consume();
                    SystemAPI.SetComponent(rpc,command);
                }
            }
        }





        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    } 
}


