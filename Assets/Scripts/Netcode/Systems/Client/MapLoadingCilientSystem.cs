using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct MapLoadingCilientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ReceiveRpcCommandRequest>().WithAny<FixedChunk,FixedBuildingObjects>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
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
            for (int i = 0; i < FixedBuildingObjects.size; i++)
            {
                int value = buildingObjects[i];
                if (value != 0)
                {
                    byte[] bytes = new byte[value];
                    for (int j = i + 1; j <=  value + i; j++)
                    {
                        bytes[j - i - 1] = buildingObjects[j];
                    }
                    GridObject gridObject = new GridObject(bytes);
                    Debug.Log("Noewe " + gridObject.ToString());
                    i += value;
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }




        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
