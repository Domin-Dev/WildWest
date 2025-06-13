using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct MapLoadingCilientSystem : ISystem
{
    public bool kk;
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ReceiveRpcCommandRequest>().WithAny<FixedChunk,FixedBuildingObjects>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
        kk = false;
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
   
        foreach ((FixedChunk chunkStruct, ReceiveRpcCommandRequest receiveRpc, Entity entity) in
        SystemAPI.Query<FixedChunk, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            MapVisualization.instance.CreateMesh(chunkStruct);
            entityCommandBuffer.DestroyEntity(entity);
        }



        foreach ((FixedBuildingObjects buildingObjects, ReceiveRpcCommandRequest receiveRpc, Entity entity) in
        SystemAPI.Query<FixedBuildingObjects, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Debug.Log("/////////////// " + buildingObjects.chunkCoordinates);
            for (int i = 0; i < FixedBuildingObjects.size; i++)
            {
                int value = buildingObjects[i];
                if (value != 0)
                {
                    byte[] bytes = new byte[value];
                    byte[] bytes2 = new byte[8];
                    for (int j = i + 1; j <= 8 + i; j++)
                    {
                        bytes2[j - i - 1] = buildingObjects[j];
                    }
                    for (int j = i + 9; j <=  value + i + 8; j++)
                    {
                        bytes[j - i - 9] = buildingObjects[j];
                    }
                    GridObject gridObject = new GridObject(bytes);
                    CreateObject(ref entityCommandBuffer,gridObject,
                    new float2(BitConverter.ToInt32(bytes2,0) + buildingObjects.chunkCoordinates.x
                              ,BitConverter.ToInt32(bytes2,4) + buildingObjects.chunkCoordinates.y 
                    ));
                    i += value + 8;
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }




        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void CreateObject(ref EntityCommandBuffer entityCommand, GridObject gridObject, float2 pos)
    {
        Entity entity = entityCommand.CreateEntity();
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(pos.x, pos.y, pos.y));


        entityCommand.AddComponent(entity, localTransform);
        entityCommand.AddComponent(entity, new IsChanged());
        entityCommand.SetComponentEnabled(entity, typeof(IsChanged), true);
        entityCommand.AddComponent(entity, new Physics2D()
        {
            layer = 0,
            cellIndex = new int2(int.MinValue, int.MinValue)
        });
        entityCommand.AddComponent(entity, new BoxCollider2D()
        {
            offset = 0f,
            size = new float2(0.2f, 0.2f)
        });
        entityCommand.AddComponent(entity, new Velocity2D() { Value = float2.zero });
    }
}
