using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(NetworkReceiveSystemGroup))]
public partial struct NetCodeConnectionEventListener : ISystem
{
    

    public static event Action<int> OnClientDisconnected;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ServerData>();
        state.RequireForUpdate<NetworkStreamDriver>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var connectionEventsForClient = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;
        var serverData = SystemAPI.GetSingletonRW<ServerData>();

        foreach (var evt in connectionEventsForClient)
        {
            switch (evt.State)
            {
                case ConnectionState.State.Disconnected:

                    if (evt.Id.Value <= 0)
                        break;
                    OnClientDisconnected?.Invoke(evt.Id.Value);
                    SaveSystem.Save();
                    Player playerDisconnected = new Player();
                    Entity playerEntity = Entity.Null;

                    foreach ((RefRO<GhostOwner> owner, RefRO<Player> player, Entity entity) in
                    SystemAPI.Query<RefRO<GhostOwner>,RefRO<Player>>().WithEntityAccess())
                    {
                        if(owner.ValueRO.NetworkId == evt.Id.Value)
                        {
                            playerDisconnected = player.ValueRO;
                            playerEntity = entity;
                            break;
                        }
                    }

                    RPCHelper.SendRpc(ref entityCommandBuffer, new PlayerLeftRPC()
                    { 
                        messageTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        playerName = playerDisconnected.playerName,
                        ReasonCode = (byte)evt.DisconnectReason,
                    });
                    if (playerEntity != Entity.Null)
                    {
                        GlobalRelevancySystem.OnGhostDestroyed(SystemAPI.GetComponentRO<GhostInstance>(playerEntity).ValueRO.ghostId);
                        entityCommandBuffer.DestroyEntity(playerEntity);
                    }
                    break;
                case ConnectionState.State.Connected:
                    var connection = state.EntityManager.GetComponentData<NetworkStreamConnection>(evt.ConnectionEntity);
                    var driver = SystemAPI.GetSingletonRW<NetworkStreamDriver>().ValueRO;
                    var remoteEP = driver.GetRemoteEndPoint(connection);

                    Debug.Log(remoteEP.Address);


                    if (serverData.ValueRO.isHost && remoteEP.IsLoopback && serverData.ValueRO.hostNetworkID < 0)
                    {
                        entityCommandBuffer.AddComponent<Admin>(evt.ConnectionEntity);
                        serverData.ValueRW.hostNetworkID = evt.Id.Value;
                    }
                    break;
                case ConnectionState.State.Approval:
                    EntityQuery query = state.EntityManager.CreateEntityQuery(typeof(Player));
                    int playerCount = query.CalculateEntityCount();

                    if (playerCount >= serverData.ValueRO.playersLimit)
                        entityCommandBuffer.AddComponent(evt.ConnectionEntity, new NetworkStreamRequestDisconnect() { Reason = NetworkStreamDisconnectReason.ClosedByRemote});

                    if (serverData.ValueRO.isPassword && !(serverData.ValueRO.isHost && serverData.ValueRO.hostNetworkID < 0))
                    {
                        FixedString128Bytes clientSalt = AuthUtils.GetSalt();
                        entityCommandBuffer.AddComponent(evt.ConnectionEntity, new ClientSalt() { salt = clientSalt });
                        RPCHelper.SendApprovalRpc(ref entityCommandBuffer, evt.ConnectionEntity, new PlayerSaltRPC()
                        {
                            salt = clientSalt
                        });
                    }
                    else
                    {
                        entityCommandBuffer.AddComponent<ConnectionApproved>(evt.ConnectionEntity);
                        RPCHelper.SendApprovalRpc(ref entityCommandBuffer, evt.ConnectionEntity, new AuthResponse() { success = true });
                    }
                break;
            }


            UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] {evt.ToFixedString()}!");
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
