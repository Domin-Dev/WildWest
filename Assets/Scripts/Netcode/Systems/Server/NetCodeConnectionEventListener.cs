using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(NetworkReceiveSystemGroup))]
public partial struct NetCodeConnectionEventListener : ISystem
{
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
                    if (!SystemAPI.HasComponent<ConnectionApproved>(evt.ConnectionEntity))
                        break;
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
                    if(playerEntity != Entity.Null)  entityCommandBuffer.DestroyEntity(playerEntity);
                    break;
                case ConnectionState.State.Connected:
                    EntityQuery query = state.EntityManager.CreateEntityQuery(typeof(Player));
                    int playerCount = query.CalculateEntityCount();

                    if (playerCount >= serverData.ValueRO.playersLimit)
                        entityCommandBuffer.AddComponent(evt.ConnectionEntity, new NetworkStreamRequestDisconnect());
                   

                    if (serverData.ValueRO.isHost && serverData.ValueRO.hostNetworkID < 0)
                        serverData.ValueRW.hostNetworkID = evt.Id.Value;

                    if (serverData.ValueRO.isPassword && serverData.ValueRO.hostNetworkID != evt.Id.Value)
                    {
                        FixedString128Bytes clientSalt = AuthUtils.GetSalt();
                        entityCommandBuffer.AddComponent(evt.ConnectionEntity, new ClientSalt() { salt = clientSalt });
                        RPCHelper.SendRpc(ref entityCommandBuffer,evt.ConnectionEntity, new PlayerSaltRPC()
                        {
                            salt = clientSalt
                        });
                    }
                    else
                        RPCHelper.SendRpc(ref entityCommandBuffer,evt.ConnectionEntity, new AuthResponse() { success = true });   
                break;
            }


            UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] {evt.ToFixedString()}!");
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
