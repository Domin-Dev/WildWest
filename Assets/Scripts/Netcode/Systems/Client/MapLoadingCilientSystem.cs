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

    protected override void OnUpdate()
    {
        Debug.Log(MapVisualization.instance.clientMap);

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

                        CreateObject(ref ecb, gridObject, pos);

                        i += value + 8;
                    }
                }

                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }


    private void CreateObject(ref EntityCommandBuffer entityCommand, GridObject gridObject, float2 pos)
    {
        Debug.Log(pos + " New Object!!!!");
        EntitySpawner.instance.SpawnBuildingObject(gridObject,pos);
        //Entity entity = entityCommand.CreateEntity();

        //var localTransform = LocalTransform.FromPosition(new float3(pos.x, pos.y, pos.y));

        //entityCommand.AddComponent(entity, localTransform);
        //entityCommand.AddComponent(entity, new IsChanged());
        //entityCommand.SetComponentEnabled<IsChanged>(entity, true);
        //entityCommand.AddComponent(entity, new Physics2D()
        //{
        //    layer = 0,
        //    cellIndex = new int2(int.MinValue, int.MinValue)
        //});
        //entityCommand.AddComponent(entity, new BoxCollider2D()
        //{
        //    offset = 0f,
        //    size = new float2(0.2f, 0.2f)
        //});
        //entityCommand.AddComponent(entity, new Velocity2D() { Value = float2.zero });
    }
}
