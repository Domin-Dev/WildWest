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
            .WithAll<NewWorldItem,WorldItem,LocalTransform>();
        entityQuery = state.GetEntityQuery(entityQueryBuilder);
        state.RequireForUpdate(entityQuery);
        state.RequireForUpdate<PhysicsWorldSingleton>();
        entityQueryBuilder.Dispose();



        worldItemLookup = SystemAPI.GetComponentLookup<WorldItem>(true);
        connections = SystemAPI.GetComponentLookup<PlayerSourceConnection>(true);
        playerNeedLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>(true);
        
    }

    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("update new world!!");
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
            if(worldItemLookup.IsComponentEnabled(e))
            {
                var worldItem = worldItemLookup[e];
                var transform = transforms[i];

                if(ItemsAsset.instance.GetStackMax(worldItem.item.itemId) <= 1)
                    continue;

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
                    continue;

                foreach(var hit in overlapHits)
                {
                    var currentHit = physicsWorldSingleton.Bodies[hit].Entity;
                    if(currentHit == e)
                        continue;
                    var worldItemHit = worldItemLookup[currentHit];
                    if(worldItem.item.itemId == worldItemHit.item.itemId)
                    {
                        RPCHelper.SendEventsToClients(new MergeItemsPRC()
                        {
                            duration = 0.4f,
                            fromChunkIndex = worldItemHit.chunkIndex,
                            fromSlotIndex = worldItemHit.slotIndex,
                            toChunkIndex = worldItem.chunkIndex,
                            toSlotIndex = worldItem.slotIndex
                        },connections,playerNeedLookup,loadedChunks,ecb,worldItem.chunkIndex,tick,true);
                    }
                    Debug.Log("hit : " + currentHit);
                }  
            }
        }




        ecb.Playback(state.EntityManager);  
        ecb.Dispose();
    }
}