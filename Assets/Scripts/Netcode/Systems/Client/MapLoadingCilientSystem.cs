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
      //  entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    }


    private float timer = 0f;
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        timer += deltaTime;

        if (timer < 0.25f) return; // wykonuj co 1 sekundê
        timer = 0f;


        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mapVis = MapVisualization.instance;
       
        //Entities
        //    .WithAll<ReceiveRpcCommandRequest, FixedChunk>()
        //    .ForEach((Entity entity, in FixedChunk chunkStruct) =>
        //    {
        //        clientMap.AddChunk(chunkStruct);
        //        ecb.DestroyEntity(entity);
        //    }).WithoutBurst().Run();


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
                       Debug.Log(counter.index + "akcja!" + ev.flags);

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
            Entities.ForEach((Entity e, ChunkComponent chunk) =>
            {
                if(chunksToLoad.Contains(chunk.index))
                {
                    Debug.Log("find!");
                    clientMap.AddChunk(chunk.index, e);
                    chunksToLoad.Remove(chunk.index);
                }
            }).WithoutBurst().Run();
        }
        mapVis?.RenderNewChunks();


        //Entities
        //    .WithAll<ReceiveRpcCommandRequest,FixedBuildingObjects>()
        //    .ForEach((Entity entity, in FixedBuildingObjects buildingObjects) =>
        //    {
        //        for (int i = 0; i < FixedBuildingObjects.size; i++)
        //        {
        //            int value = buildingObjects[i];
        //            if (value != 0)
        //            {
        //                byte[] bytes = new byte[value];
        //                byte[] bytes2 = new byte[8];

        //                for (int j = i + 1; j <= 8 + i; j++)
        //                    bytes2[j - i - 1] = buildingObjects[j];

        //                for (int j = i + 9; j <= value + i + 8; j++)
        //                    bytes[j - i - 9] = buildingObjects[j];

        //                var gridObject = new GridObject(bytes);
        //                var pos = new float2(
        //                    BitConverter.ToInt32(bytes2, 0) + buildingObjects.chunkCoordinates.x,
        //                    BitConverter.ToInt32(bytes2, 4) + buildingObjects.chunkCoordinates.y
        //                );


        //                BuildingObjectCreator.CreateObject(ref entitiesReferences, EntityManager, ref ecb, gridObject, pos);

        //                i += value + 8;
        //            }
        //        }

        //        ecb.DestroyEntity(entity);
        //    }).WithoutBurst().Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
