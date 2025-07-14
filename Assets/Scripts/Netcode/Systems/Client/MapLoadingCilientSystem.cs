using Game.Client.Map;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class MapLoadingClientSystem : SystemBase
{
    private ClientMap clientMap;
    private EntitiesReferences entitiesReferences;

    public void SetUp()
    {
        clientMap = new ClientMap();
        MapVisualization.instance.map = clientMap;
        Debug.Log(MapVisualization.instance.clientMap);
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        var entityQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ReceiveRpcCommandRequest) },
            Any = new ComponentType[] { typeof(FixedChunk), typeof(FixedBuildingObjects) }
        };
        RequireForUpdate(GetEntityQuery(entityQueryDesc));
    }

    protected override void OnStartRunning()
    {
        entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mapVis = MapVisualization.instance;

        Entities
            .WithAll<ReceiveRpcCommandRequest, FixedChunk>()
            .ForEach((Entity entity, in FixedChunk chunkStruct) =>
            {
                clientMap.AddChunk(chunkStruct);
                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
        mapVis.RenderNewChunks();

        Entities
            .WithAll<ReceiveRpcCommandRequest,FixedBuildingObjects>()
            .ForEach((Entity entity, in FixedBuildingObjects buildingObjects) =>
            {

                Debug.Log("Pos " + buildingObjects.chunkCoordinates);
                for (int i = 0; i < FixedBuildingObjects.size; i++)
                {
                    int value = buildingObjects[i];
                    if (value != 0)
                    {
                        byte[] bytes = new byte[value];
                        byte[] bytes2 = new byte[8];

                        for (int j = i + 1; j <= 8 + i; j++)
                            bytes2[j - i - 1] = buildingObjects[j];

                        for (int j = i + 9; j <= value + i + 8; j++)
                            bytes[j - i - 9] = buildingObjects[j];

                        var gridObject = new GridObject(bytes);
                        var pos = new float2(
                            BitConverter.ToInt32(bytes2, 0) + buildingObjects.chunkCoordinates.x,
                            BitConverter.ToInt32(bytes2, 4) + buildingObjects.chunkCoordinates.y
                        );



                        
                        BuildingObjectCreator.CreateObject(ref entitiesReferences, EntityManager, ref ecb, gridObject, pos);

                        i += value + 8;
                    }
                }

                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
