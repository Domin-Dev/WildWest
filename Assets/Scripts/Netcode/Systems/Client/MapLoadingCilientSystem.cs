using System;
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
            .WithAll<FixedChunk, ReceiveRpcCommandRequest>();
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
           // entityCommandBuffer.DestroyEntity(entity);
            entityCommandBuffer.RemoveComponent<ReceiveRpcCommandRequest>(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
