using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GetPlayerListServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GetPlayerDashboardRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        NativeList<PlayerList> playerLists = new NativeList<PlayerList>(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, GetPlayerDashboardRPC requestRPC, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, GetPlayerDashboardRPC>().WithEntityAccess())
        {
            if(playerLists.IsEmpty)
                GetPlayers(ref state,ref playerLists, ref entityCommandBuffer);

            for (int i = 0; i < playerLists.Length; i++)
            {
                PlayerList playerList = playerLists[i];
                playerList.max = (short)playerLists.Length;
                RPCHelper.SendRpc(ref entityCommandBuffer, rpcCommandRequest.ValueRO.SourceConnection, playerList);
            }

            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
        playerLists.Dispose();
    }

    private void GetPlayers(ref SystemState state,ref NativeList<PlayerList> playerLists, ref EntityCommandBuffer entityCommandBuffer)
    {
        int index = 0;
        PlayerList playerList = new PlayerList();

        foreach ((RefRO<Player> player, RefRO<PlayerLook> look, RefRO<GhostOwner> owner) in
        SystemAPI.Query<RefRO<Player>, RefRO<PlayerLook>, RefRO<GhostOwner>>())
        {
            if (index > 0 && index % 6 == 0)
            {
                playerLists.Add(playerList);
                playerList = new PlayerList();
            }
            playerList[index % 6] = new PlayerData()
            {
                playerID = owner.ValueRO.NetworkId,
                playerName = player.ValueRO.playerName,
                characterLook = look.ValueRO.look
            };
            index++;
        }
        playerLists.Add(playerList);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
