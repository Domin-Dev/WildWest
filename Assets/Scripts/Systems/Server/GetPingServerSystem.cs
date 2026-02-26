using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GetPingServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GetPingRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        NativeList<PingList> playerLists = new NativeList<PingList>(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, GetPingRPC requestRPC, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, GetPingRPC>().WithEntityAccess())
        {
            if(playerLists.IsEmpty)
                GetPing(ref state,ref playerLists, ref entityCommandBuffer);

            for (int i = 0; i < playerLists.Length; i++)
            {
                PingList playerList = playerLists[i];
                RPCHelper.SendRpc(ref entityCommandBuffer, rpcCommandRequest.ValueRO.SourceConnection, playerList);
            }

            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
        playerLists.Dispose();
    }

    private void GetPing(ref SystemState state,ref NativeList<PingList> playerLists, ref EntityCommandBuffer entityCommandBuffer)
    {
        int index = 0;
        PingList playerList = new PingList();
        playerList.length = 0;

        foreach ((RefRO<NetworkSnapshotAck> ping, RefRO<NetworkId> owner) in
        SystemAPI.Query< RefRO<NetworkSnapshotAck>, RefRO<NetworkId>>().WithAll<NetworkStreamInGame>())
        {
            if (index > 0 && index % 100 == 0)
            {
                playerLists.Add(playerList);
                playerList = new PingList();
                playerList.length = 0;
            }
            int i = index % 100;
            playerList[i] = new PingData()
            {
                playerID = owner.ValueRO.Value,
                ping = (int)ping.ValueRO.EstimatedRTT
            };
            playerList.length++;
            index++;
        }
        playerLists.Add(playerList);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
