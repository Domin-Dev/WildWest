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

    public void OnCreate(ref SystemState state)
    {
        if (ClientServerBootstrap.HasClientWorlds)
        {
            SystemAPI.GetSingletonRW<ServerData>().ValueRW.hostNetworkID = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntityQuery(typeof(NetworkId)).GetSingleton<NetworkId>().Value;
            Debug.Log("dzial!!!!");
        }
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var connectionEventsForClient = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;
        foreach (var evt in connectionEventsForClient)
        {
            switch (evt.State)
            {
                case ConnectionState.State.Disconnected:
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
                    entityCommandBuffer.DestroyEntity(playerEntity);
                    break;
                case ConnectionState.State.Connected:
                    var serverData = SystemAPI.GetSingletonRW<ServerData>();

                    if (serverData.ValueRO.isHost && serverData.ValueRO.hostNetworkID < 0)
                        serverData.ValueRW.hostNetworkID = evt.Id.Value;

                    if (serverData.ValueRO.isPassword && serverData.ValueRO.hostNetworkID != evt.Id.Value)
                    {
                        Debug.Log("nowa sol!");
                        RPCHelper.SendRpc(ref entityCommandBuffer, new PlayerSaltRPC()
                        {
                            salt = AuthUtils.GetSalt()
                        });
                    }
                    else
                        RPCHelper.SendRpc(ref entityCommandBuffer, new AuthResponse() { success = true });   
                break;
            }

            UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] {evt.ToFixedString()}!");
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
