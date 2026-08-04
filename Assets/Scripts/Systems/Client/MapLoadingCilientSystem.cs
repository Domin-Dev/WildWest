using Game.Client.Map;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[DisableAutoCreation]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class MapLoadingClientSystem : SystemBase
{
    private ClientMap clientMap;
    private EntitiesReferences entitiesReferences;
    private NativeHashSet<int> chunksToLoad;


    private BufferLookup<BuildingObjects> objectsLookup;
    private BufferLookup<LocalBuildingObjects> localObjectsLookup;
    private BufferLookup<LinkedEntityGroup> linkedLookup;

    public void SetUp()
    {
        clientMap = new ClientMap();
        MapVisualization.instance.clientMap = clientMap;
    }

    public void SetMapSettings(MapIsLoaded map)
    { 
    //    clientMap.widthInChunks = map.widthInChunks;
    }
    protected override void OnCreate()
    {
        base.OnCreate();
        chunksToLoad = new NativeHashSet<int>(30,Allocator.Persistent);
        RequireForUpdate<EntitiesReferences>();
        RequireForUpdate<Chunks>();
        //var entityQueryDesc = new EntityQueryDesc
        //{
        //    All = new ComponentType[] { typeof(ReceiveRpcCommandRequest) },
        //    Any = new ComponentType[] { typeof(FixedChunk), typeof(FixedBuildingObjects) }
        //};
        //RequireForUpdate(GetEntityQuery(entityQueryDesc));

        objectsLookup = SystemAPI.GetBufferLookup<BuildingObjects>();
        localObjectsLookup = SystemAPI.GetBufferLookup<LocalBuildingObjects>();
        linkedLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>();
    }
    protected override void OnDestroy()
    {
        chunksToLoad.Dispose();
    }
    protected override void OnStartRunning()
    {
        entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    }

    private float timer = 0f;
    protected override void OnUpdate()
    {
        // float deltaTime = SystemAPI.Time.DeltaTime;
        // timer += deltaTime;

        // if (timer < 0.05) return; 
        // timer = 0f;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mapVis = MapVisualization.instance;
        var chunks = SystemAPI.GetSingleton<Chunks>();
        objectsLookup.Update(this);
        localObjectsLookup.Update(this);
        linkedLookup.Update(this);


        bool mapIsUpdated = false;
       Entities
       .ForEach((Entity e,ChunkEventCounter counter, DynamicBuffer<ChunkEvents> events) =>
       {
           if(!events.IsEmpty)
           {
            while (true)
           {
             //  bool isEvent = false;
               for (int i = 0; i < events.Length; i++)
               {
                   var ev = events[i];
                   if (ev.index == counter.index)
                   {
                       counter.index++;
                       switch (ev.flags)
                       {
                            case ChunkEventType.LoadChunk:
                               chunksToLoad.Add(ev.chunk);
                               break;
                            case ChunkEventType.UnloadChunk:
                               clientMap.RemoveChunk(ev.chunk);
                               break;
                            case ChunkEventType.UpdateBuildingObject:
                                mapIsUpdated = true;
                                if(chunks.currentChunks.TryGetValue(ev.chunk,out Entity chunkEntity))  
                                {
                                    if(EntityHelper.TryFindBuildingObject(chunkEntity,ev.tilePosition,objectsLookup,out var result,out int index))
                                    {
                                        if(EntityHelper.TryFindBuildingObject(chunkEntity,ev.tilePosition,localObjectsLookup,out var localResult, out int localIndex))
                                        {
                                            float hp = (float)result.hitPoints / result.maxHitPoints;
                                            int condition = Mathf.Clamp((int)((1f - hp) * 3f), 0, 2);
                                            EntityManager.GetComponentObject<SpriteRenderer>(localResult.localSpriteEntity).sprite = ItemsAsset.instance.GetBuildingObjectSprite(result.id,result.variantIndex,condition);
                                        }
                                    }
                                    else if(EntityHelper.TryFindBuildingObject(chunkEntity,ev.tilePosition,localObjectsLookup,out var localResult, out int localIndex))
                                    {
                                        var buffer = linkedLookup[chunkEntity];
                                        for (int k = buffer.Length - 1; k >= 0; k--)
                                        {
                                            if (buffer[k].Value == localResult.localEntity)
                                            {
                                                buffer.RemoveAtSwapBack(k);
                                                break;
                                            }
                                        }
                                        if(ItemsAsset.instance.TryGetItem<BuildingObject>(localResult.id,out var itemData))
                                            Sounds.instance.PlayerSound(itemData.destructionSound);

                                        ecb.AddComponent<DestroyEntityTag>(localResult.localEntity);
                                        localObjectsLookup[chunkEntity].RemoveAtSwapBack(localIndex);
                                    }
                                }
                                break;
                       }

                       //isEvent = true;
                       break;
                   }
               }
              // if (!isEvent) break; 
              break;
           }
            ecb.SetComponent(e, counter);
           }
       })
       .WithoutBurst().Run();

        if(mapIsUpdated)
        {
            foreach (RefRW<PlayerPointer> pointer in SystemAPI.Query<RefRW<PlayerPointer>>().WithAll<Player,GhostOwnerIsLocal>())
            {
                pointer.ValueRW.updateTileInfo = true;
            }
        }

        if (!chunksToLoad.IsEmpty)
        {
            Entities.ForEach((Entity e, ChunkComponent chunk, DynamicBuffer<BuildingObjects> buildingObjects, DynamicBuffer<LocalBuildingObjects> localBuildingObjects) =>
            {
                if(chunksToLoad.Contains(chunk.chunkIndex))
                {
                    clientMap.AddChunk(chunk.chunkIndex, e);
                    chunksToLoad.Remove(chunk.chunkIndex);

                    for(int i = 0; i < buildingObjects.Length; i++)
                    {
                        var item = buildingObjects[i];
                        Entity obj = BuildingObjectCreator.CreateObject(entitiesReferences,EntityManager,ecb,item,out Entity spriteEntity);   
                        ecb.AppendToBuffer<LinkedEntityGroup>(e,obj);
                        ecb.AppendToBuffer(e,new LocalBuildingObjects()
                        {
                            globalTilePos = item.globalTilePos,
                            localEntity = obj,
                            localSpriteEntity = spriteEntity,
                            id = item.id
                        });
                        item.localEntity = obj;
                        buildingObjects.ElementAt(i) = item;
                    }
                }
            }).WithoutBurst().Run();
        }
        mapVis?.RenderNewChunks();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
