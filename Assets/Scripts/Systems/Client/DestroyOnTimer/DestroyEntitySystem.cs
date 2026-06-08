using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
public partial class DestroyEntitySystem : SystemBase
{


    private BufferLookup<ChunkObjects> chunkObjects;


    protected override void OnCreate()
    {
        RequireForUpdate<EndPredictedSimulationEntityCommandBufferSystem.Singleton>();
        RequireForUpdate<NetworkTime>();
        chunkObjects = SystemAPI.GetBufferLookup<ChunkObjects>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
       .WithAll<DestroyEntityTag>();
        RequireForUpdate(GetEntityQuery(entityQueryBuilder));
    }

    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        chunkObjects.Update(this);
        var current = networkTime.ServerTick;    
        var ecbSingleton = SystemAPI.GetSingleton<EndPredictedSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);


        foreach (var(localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<DestroyEntityTag>().WithEntityAccess())
        {
            Debug.Log("niszczenie!!!");
            if(World.IsServer())
            {
                if (SystemAPI.HasComponent<Bullet>(entity)) HybridManager.instance.EntityDeleted(entity);
                if (SystemAPI.HasComponent<GhostInstance>(entity)) GlobalRelevancySystem.OnGhostDestroyed(EntityManager.GetComponentData<GhostInstance>(entity).ghostId);
                if (SystemAPI.HasComponent<GhostChunk>(entity))
                {
                    var ghostChunk = SystemAPI.GetComponentRO<GhostChunk>(entity);

                    if(SystemAPI.Exists(ghostChunk.ValueRO.currentChunkEntity))
                    {
                        var buffer = chunkObjects[ghostChunk.ValueRO.currentChunkEntity];
                        for(int i = 0; i < buffer.Length; i++)
                        {
                            if(buffer[i].entity == entity)
                            {
                                buffer.RemoveAtSwapBack(i);
                                break;
                            }
                        }
                    }
                }
                
                ecb.DestroyEntity(entity);
            }
            else
            {
                if(SystemAPI.HasComponent<Bullet>(entity)) HybridManager.instance.EntityDeleted(entity);  
                localTransform.ValueRW.Position = new float3(100000, 100000,100000);
                //ecb.RemoveComponent<Simulate>(entity);
                if (!EntityManager.HasComponent<GhostInstance>(entity))
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}

