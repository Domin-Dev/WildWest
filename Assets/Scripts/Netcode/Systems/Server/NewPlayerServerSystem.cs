using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct NewPlayerServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewPlayerJoinRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, NewPlayerJoinRPC newPlayer, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, NewPlayerJoinRPC>().WithEntityAccess())
        {
            RPCHelper.SendRpc(ref entityCommandBuffer, rpcCommandRequest.ValueRO.SourceConnection, new LifeStatsChangedRPC());

            entityCommandBuffer.AddComponent(rpcCommandRequest.ValueRO.SourceConnection, new PlayerName() { name = newPlayer.playerName });
            entityCommandBuffer.AddComponent(rpcCommandRequest.ValueRO.SourceConnection, new SendMap() { position = new float2(0.5f, 0.5f) });
            entityCommandBuffer.DestroyEntity(entity);           
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
