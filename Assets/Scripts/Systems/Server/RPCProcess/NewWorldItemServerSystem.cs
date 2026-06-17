using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;



[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct NewWorldItemServerSystem : ISystem
{

    private ComponentLookup<WorldItem> worldItemLookup;
    private ComponentLookup<PlayerSourceConnection> connections;
    private BufferLookup<PlayersNeedChunk> playerNeedLookup;
    private EntityQuery entityQuery;


    public void OnCreate(ref SystemState state)
    {
       state.RequireForUpdate<WorldItemsConfig>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewWorldItem,LocalTransform>().WithDisabled<WorldItem>();
        entityQuery = state.GetEntityQuery(entityQueryBuilder);
        state.RequireForUpdate(entityQuery);
        state.RequireForUpdate<PhysicsWorldSingleton>();
        entityQueryBuilder.Dispose();



        worldItemLookup = SystemAPI.GetComponentLookup<WorldItem>();
        connections = SystemAPI.GetComponentLookup<PlayerSourceConnection>(true);
        playerNeedLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>(true);
        
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<WorldItemsConfig>();
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        worldItemLookup.Update(ref state);
        playerNeedLookup.Update(ref state);
        connections.Update(ref state);


        float3 size = new float3(config.sizeWorldItemCollider,0) * 0.5f;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        
        var entities = entityQuery.ToEntityArray(state.WorldUpdateAllocator);
        var transforms = entityQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
        var loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        for(int i = 0; i < entities.Length; i++)
        {
            var e = entities[i];
            ecb.RemoveComponent<NewWorldItem>(e);
            var worldItem = worldItemLookup.GetRefRW(e);
            var transform = transforms[i];
            if(ItemsAsset.instance.GetStackMax(worldItem.ValueRO.item.itemId) <= 1)
            {
                worldItemLookup.SetComponentEnabled(e,true);
                continue;
            }

            var aabbInput = new OverlapAabbInput()
            {
                Aabb = new Aabb()
                {
                    Min = transform.Position - size,
                    Max = transform.Position + size,

                },
                Filter = config.FilterToFindSimilarWorldItems
            };

            var overlapHits = new NativeList<int>(state.WorldUpdateAllocator);
            if(!physicsWorldSingleton.OverlapAabb(aabbInput,ref overlapHits))
            {
                worldItemLookup.SetComponentEnabled(e,true);
                continue;
            }

            bool isMerge = false; 
            foreach(var hit in overlapHits)
            {
                var currentHit = physicsWorldSingleton.Bodies[hit].Entity;
                if(currentHit == e || !worldItemLookup.EntityExists(currentHit))
                    continue;
                var worldItemHit = worldItemLookup.GetRefRW(currentHit);

                if(worldItem.ValueRO.item.itemId == worldItemHit.ValueRO.item.itemId && worldItemHit.ValueRW.mergeCounter >= 0 && !SystemAPI.HasComponent<DestroyEntityTag>(currentHit))
                {
                    RPCHelper.SendEventsToClients(new MergeItems()
                    {
                        mergeItemsPRC = new MergeItemsPRC()
                        {
                            duration = 0.4f,
                            fromChunkIndex = worldItemHit.ValueRO.chunkIndex,
                            fromSlotIndex = worldItemHit.ValueRO.slotIndex,
                            toChunkIndex = worldItem.ValueRO.chunkIndex,
                            toSlotIndex = worldItem.ValueRO.slotIndex
                        },
                        worldItemFrom =  currentHit,
                        worldItemTo = e
                    },connections,playerNeedLookup,loadedChunks,ecb,worldItem.ValueRO.chunkIndex,tick,true);


                    worldItemHit.ValueRW.mergeCounter--;
                    worldItem.ValueRW.mergeCounter++;

                    worldItemLookup.SetComponentEnabled(currentHit,false); 
                    worldItemLookup.SetComponentEnabled(e,false);
                    isMerge = true;
                }
            }  

            if(!isMerge)
                worldItemLookup.SetComponentEnabled(e,true);    
        }

        ecb.Playback(state.EntityManager);  
        ecb.Dispose();
    }
}