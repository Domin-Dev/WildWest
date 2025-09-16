using Game.Client.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


[DisableAutoCreation]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class MapLoadingClientSystem : SystemBase
{
    private ClientMap clientMap;
    private EntitiesReferences entitiesReferences;
    private NativeHashSet<int> chunksToLoad;

    public void SetUp()
    {
        clientMap = new ClientMap();
        MapVisualization.instance.map = clientMap;
    }

    public void SetMapSettings(MapIsLoaded map)
    { 
        clientMap.widthInChunks = map.widthInChunks;
    }
    protected override void OnCreate()
    {
        base.OnCreate();
        chunksToLoad = new NativeHashSet<int>(30,Allocator.Persistent);
        RequireForUpdate<EntitiesReferences>();
        //var entityQueryDesc = new EntityQueryDesc
        //{
        //    All = new ComponentType[] { typeof(ReceiveRpcCommandRequest) },
        //    Any = new ComponentType[] { typeof(FixedChunk), typeof(FixedBuildingObjects) }
        //};
        //RequireForUpdate(GetEntityQuery(entityQueryDesc));
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
        float deltaTime = SystemAPI.Time.DeltaTime;
        timer += deltaTime;

        if (timer < 0.25f) return; 
        timer = 0f;
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mapVis = MapVisualization.instance;

       Entities
       .ForEach((Entity e,ChunkEventCounter counter, DynamicBuffer<ChunkEvents> events) =>
       {
           if (events.IsEmpty) return;
           while (true)
           {
               bool isEvent = false;
               for (int i = 0; i < events.Length; i++)
               {
                   var ev = events[i];
                   if (ev.index == counter.index)
                   {
                       counter.index++;

                     // Debug.Log(counter.index + "akcja!" + ev.flags);

                       switch (ev.flags)
                       {
                           case 1:
                               chunksToLoad.Add(ev.value.x);
                               break;
                           case 2:
                               clientMap.RemoveChunk(ev.value.x);
                               break;
                       }

                       isEvent = true;
                       break;
                   }
               }
               if (!isEvent) break; 
           }
           ecb.SetComponent(e, counter);
       })
       .WithoutBurst().Run();

        if (!chunksToLoad.IsEmpty)
        {
            Entities.ForEach((Entity e, ChunkComponent chunk, DynamicBuffer<BuildingObjects> buildingObjects, DynamicBuffer<LinkedEntityGroup> linkedEntities) =>
            {
                if(chunksToLoad.Contains(chunk.index))
                {
                    clientMap.AddChunk(chunk.index, e);
                    chunksToLoad.Remove(chunk.index);

                    foreach (var item in buildingObjects)
                    {
                        Entity bObject = BuildingObjectCreator.CreateObject(ref entitiesReferences, EntityManager, ref ecb,item );
                        linkedEntities.Add(bObject);
                    }
                }
            }).WithoutBurst().Run();
        }
        mapVis?.RenderNewChunks();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
