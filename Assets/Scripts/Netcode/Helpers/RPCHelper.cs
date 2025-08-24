using System;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;

public static class RPCHelper
{
    public static void SendRpc<T>(ref EntityCommandBuffer ecb, Entity connectionEntity, in T rpcCommand)
        where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, rpcCommand);
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
    }

    public static void SendRpc<T>(ref EntityCommandBuffer ecb, in T rpcCommand)
    where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, rpcCommand);
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());
    }

    public static void SendRpc<T>(ref EntityCommandBuffer ecb)
      where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, new  T());
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());
    }




    public static void SendMessageToClients(ref EntityCommandBuffer ecb, string serverMessage)
    {
        var rpc = new NewMessageServerRPC()
        {
            message = serverMessage,
            sender = "Server",
            messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            senderIsServer = true
        };
        SendRpc(ref ecb, rpc);
    }
    private static void DisconnectAllClients(World serverWorld)
    {
        var em = serverWorld.EntityManager;
        var connections = em.CreateEntityQuery(typeof(NetworkStreamConnection))
                            .ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var conn in connections)
        {
            em.AddComponent<NetworkStreamRequestDisconnect>(conn);
        }

        connections.Dispose();
    }

    public static void StopServer(World serverWorld)
    {
        DisconnectAllClients(serverWorld);  
        serverWorld.QuitUpdate = true; 
        serverWorld.Dispose();         
    }
}
