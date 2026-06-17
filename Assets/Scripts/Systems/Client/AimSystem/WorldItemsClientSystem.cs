using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using UnityEngine.XR;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[RequireMatchingQueriesForUpdate]
partial struct WorldItemsClientSystem : ISystem
{
    private BufferLookup<WorldItemEntity> worldItemsLookup;
    private ComponentLookup<LocalTransform> transformLookup;
    private BufferLookup<EntityContainers> containersLookup;
    private BufferLookup<InventorySlot> slotsLookup;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<Chunks>();

        transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        worldItemsLookup = SystemAPI.GetBufferLookup<WorldItemEntity>();
        containersLookup = SystemAPI.GetBufferLookup<EntityContainers>(true);
        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>(true);

        state.RequireForUpdate<VisualEffectsBuffer>();
        state.RequireForUpdate<Players>();
    }



    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        transformLookup.Update(ref state);
        worldItemsLookup.Update(ref state);
        containersLookup.Update(ref state);
        slotsLookup.Update(ref state);



        state.CompleteDependency();
        var currentTime = SystemAPI.GetSingleton<NetworkTime>();
        var prefabs = SystemAPI.GetSingleton<EntitiesReferences>();
        var chunks = SystemAPI.GetSingleton<Chunks>();
        var players = SystemAPI.GetSingleton<Players>();
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        

        foreach ((RefRO<MergeItemsPRC> action,Entity rpc) in SystemAPI.Query<RefRO<MergeItemsPRC>>().WithEntityAccess())
        {    
            if(currentTime.InterpolationTick.IsNewerThan(action.ValueRO.tick))
            {
                Entity chunkFrom;
                Entity chunkTo;

                WorldItemEntity from = new WorldItemEntity();
                WorldItemEntity to = new WorldItemEntity();

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

                if(EQHelper.TryGetBufferIndex(worldItemsLookup,action.ValueRO.fromSlotIndex,chunkFrom,out var worldItem,out int bufferId))
                    worldItemsLookup[chunkFrom].RemoveAtSwapBack(bufferId);

                if(transformLookup.TryGetComponent(from.worldItem,out var transform))
                {
                    ecb.AddComponent<MoveToTarget>(from.worldItem,new MoveToTarget()
                    {
                        duration = action.ValueRO.duration,
                        startTick = action.ValueRO.tick,
                        targetEntity = to.worldItem,
                        destroy = true,
                        startPosition = new float2(transform.Position.x,transform.Position.y)
                    });
                }        
                ecb.DestroyEntity(rpc);
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

        foreach ((RefRO<DropItemRPC> action,Entity rpc) in SystemAPI.Query<RefRO<DropItemRPC>>().WithEntityAccess())
        {    
            if(currentTime.InterpolationTick.IsNewerThan(action.ValueRO.tick))
            {
                Sounds.instance.Click();
                if(players.hashMap.TryGetValue(action.ValueRO.networkID,out Entity player))
                {
                    if(chunks.currentChunks.TryGetValue(action.ValueRO.chunkIndex,out Entity chunkEntity))
                    {
                        var container = EQHelper.GetContainer(containersLookup, chunkEntity, EquipmentConfig.chunkItems_ContainerIndex);
                        if(container.HasValue)
                        {
                            EQHelper.TryGetBufferIndex(slotsLookup,action.ValueRO.slotIndex,container.Value.entity,out InventorySlot? slot,out int bufferIndex);  
                            if(slot.HasValue)
                            {
                                LocalTransform localTransform = transformLookup[player];
                                Entity worldItem = state.EntityManager.Instantiate(prefabs.worldItemEntity);
                                Entity spriteEntity = state.EntityManager.GetBuffer<LinkedEntityGroup>(worldItem)[1].Value;
                                SpriteRenderer spriteRenderer =  state.EntityManager.GetComponentObject<SpriteRenderer>(spriteEntity);
                                spriteRenderer.sprite = ItemsAsset.instance.GetIcon(slot.Value.itemId);
                                Color? color = slot.Value.color.ConvertToUnityColor();
                                HeroEditor.SetMaterialColor(spriteRenderer,"_Color",color.HasValue ? color.Value : Color.white);
                                ecb.SetComponent(worldItem, LocalTransform.FromPosition(localTransform.Position));
                                ecb.AddComponent<MoveToTarget>(worldItem,new MoveToTarget()
                                {
                                    duration = action.ValueRO.duration,
                                    startTick = action.ValueRO.tick,
                                    target = action.ValueRO.dropPosition,
                                    startPosition = new float2(localTransform.Position.x,localTransform.Position.y)
                                });
                                ecb.SetComponent<WorldItem>(worldItem,new WorldItem()
                                {
                                    chunkIndex =  action.ValueRO.chunkIndex,
                                    slotIndex = action.ValueRO.slotIndex
                                });
                                ecb.AppendToBuffer(chunkEntity,new WorldItemEntity()
                                {
                                    slot = action.ValueRO.slotIndex,
                                    worldItem = worldItem
                                });            
                            }
                        }
                    }
                }
                ecb.DestroyEntity(rpc);
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

        foreach ((RefRO<PickUpItemRPC> action,Entity rpc) in SystemAPI.Query<RefRO<PickUpItemRPC>>().WithEntityAccess())
        {    
            if(currentTime.InterpolationTick.IsNewerThan(action.ValueRO.tick))
            {
                if(players.hashMap.TryGetValue(action.ValueRO.networkID,out Entity player) && 
                chunks.currentChunks.TryGetValue(action.ValueRO.chunkIndex,out Entity chunkEntity))
                {
                    if(EQHelper.TryGetBufferIndex(worldItemsLookup,action.ValueRO.slotIndex,chunkEntity,out var worldItem, out int bufferIndex))
                    {
                        if(transformLookup.TryGetComponent(worldItem.Value.worldItem,out var transform))
                        {
                            ecb.AddComponent<MoveToTarget>(worldItem.Value.worldItem,new MoveToTarget()
                            {
                                duration = action.ValueRO.duration,
                                startTick = action.ValueRO.tick,
                                targetEntity = player,
                                destroy = action.ValueRO.destroyItem,
                                startPosition = new float2(transform.Position.x,transform.Position.y)
                            });

                            SlotPosition slotPosition = new SlotPosition(EquipmentConfig.chunkItems_ContainerIndex,action.ValueRO.slotIndex);
                            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,chunkEntity,slotPosition,out var item,out bufferIndex))
                            {
                                RPCHelper.CreateLocalEvent(new PickUpItemCompletedClient() {item = item.Value },ecb,EntityHelper.AddTime(tick,action.ValueRO.duration));
                            }
                        }
                        worldItemsLookup[chunkEntity].RemoveAtSwapBack(bufferIndex);
                    }
                }
                ecb.DestroyEntity(rpc);
            }
            else if(SystemAPI.HasComponent<ReceiveRpcCommandRequest>(rpc))
            {
                var command = SystemAPI.GetComponent<ReceiveRpcCommandRequest>(rpc);
                if(!command.IsConsumed)
                {
                    command.Consume();
                    Debug.Log("pick up!!! " + action.ValueRO.slotIndex  + " "  + action.ValueRO.chunkIndex);
                    SystemAPI.SetComponent(rpc,command);
                }
            }
        }

        // foreach ((RefRO<CreateWorldItemRPC> action,Entity rpc) in SystemAPI.Query<RefRO<CreateWorldItemRPC>>().WithEntityAccess())
        // {    
        //     if(currentTime.InterpolationTick.IsNewerThan(action.ValueRO.tick))
        //     {
        //         if(chunks.currentChunks.TryGetValue(action.ValueRO.chunkIndex,out Entity chunkEntity))
        //         {
        //             var container = EQHelper.GetContainer(containersLookup, chunkEntity, EquipmentConfig.chunkItems_ContainerIndex);
        //             if(container.HasValue && EQHelper.TryGetBufferIndex(slotsLookup,action.ValueRO.slotIndex,container.Value.entity,out InventorySlot? slot,out int bufferIndex))
        //             {
        //                 Entity worldItem = state.EntityManager.Instantiate(prefabs.worldItemEntity);
        //                 Entity spriteEntity = state.EntityManager.GetBuffer<LinkedEntityGroup>(worldItem)[1].Value;
        //                 SpriteRenderer spriteRenderer =  state.EntityManager.GetComponentObject<SpriteRenderer>(spriteEntity);
        //                 spriteRenderer.sprite = ItemsAsset.instance.GetIcon(slot.Value.itemId);
        //                 Color? color = slot.Value.color.ConvertToUnityColor();
        //                 HeroEditor.SetMaterialColor(spriteRenderer,"_Color",color.HasValue ? color.Value : Color.white);
        //                 ecb.SetComponent(worldItem, LocalTransform.FromPosition(MyTools.ConvertFloat(action.ValueRO.position)));
        //                 ecb.SetComponent<WorldItem>(worldItem,new WorldItem()
        //                 {
        //                     chunkIndex = action.ValueRO.chunkIndex,
        //                     slotIndex = action.ValueRO.slotIndex
        //                 });
        //                 ecb.AppendToBuffer(chunkEntity,new WorldItemEntity()
        //                 {
        //                     slot = action.ValueRO.slotIndex,
        //                     worldItem = worldItem
        //                 });
        //             }
        //         }
        //         ecb.DestroyEntity(rpc);
        //     }
        //     else if(SystemAPI.HasComponent<ReceiveRpcCommandRequest>(rpc))
        //     {
        //         var command = SystemAPI.GetComponent<ReceiveRpcCommandRequest>(rpc);
        //         if(!command.IsConsumed)
        //         {
        //             command.Consume();
        //             SystemAPI.SetComponent(rpc,command);
        //         }
        //     }
        // }

        


 
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    } 
}


