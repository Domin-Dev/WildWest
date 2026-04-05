using System;
using System.Diagnostics;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;

public static class RPCHelper
{  
    public static void SendRpc<T>(EntityCommandBuffer ecb, Entity connectionEntity, in T rpcCommand)
        where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, rpcCommand);
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
    }
    public static void SendApprovalRpc<T>(ref EntityCommandBuffer ecb, Entity connectionEntity, in T rpcCommand)
      where T : unmanaged, IApprovalRpcCommand
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

    public static void SendRpc<T>(EntityManager em, in T rpcCommand) where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = em.CreateEntity();
        em.AddComponentData(rpcEntity, rpcCommand);
        em.AddComponentData(rpcEntity, new SendRpcCommandRequest());
    }
    public static void SendRpc<T>(EntityManager em,Entity connectionEntity, in T rpcCommand) where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = em.CreateEntity();
        em.AddComponentData(rpcEntity, rpcCommand);
        em.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
    }


    public static void SendRpc<T>(ref EntityCommandBuffer ecb)
      where T : unmanaged, IRpcCommand
    {
        SendRpc(ref ecb, new T());
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
    public static void SendMessageToClient(ref EntityCommandBuffer ecb, string serverMessage, Entity client)
    {
        var rpc = new NewMessageServerRPC()
        {
            message = serverMessage,
            sender = "Server",
            messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            senderIsServer = true
        };
        SendRpc(ecb, client, rpc);
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

    public static void SendEventsToClientsAndOwner<T>(ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID, int chunkIndex, NetworkTick tick)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        SendEventsToClientsAndOwner(new T(),ref state,playerNeedChunkLookup,loadedChunks,ecb,networkID,chunkIndex,tick);
    }
    public static void SendEventsToClients<T>(ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID, int chunkIndex, NetworkTick tick)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        SendEventsToClients(new T(),ref state,playerNeedChunkLookup,loadedChunks,ecb,networkID,chunkIndex,tick);
    }
    public static void SendEventsToClients<T>(T rpc,ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID, int chunkIndex, NetworkTick tick)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        Entity chunk = Entity.Null; 
        foreach(var chunkTmp in loadedChunks)
        {
            if(chunkTmp.chunkIndex == chunkIndex)
                chunk = chunkTmp.chunkEntity;
        }
        if(chunk == Entity.Null) return;

        var players = playerNeedChunkLookup[chunk];

        tick.Add(2u);
        rpc.SetPlayer(networkID,tick);
        
        foreach(var player in players)
        {
            if(player.networkID != networkID)
            {
                var connection = state.EntityManager.GetComponentData<PlayerSourceConnection>(player.playerEntity).value;
                RPCHelper.SendRpc(ecb,connection,rpc);
            }
        }
    }
    public static void SendEventsToClientsAndOwner<T>(T rpc,ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID, int chunkIndex, NetworkTick tick)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        Entity chunk = Entity.Null; 
        foreach(var chunkTmp in loadedChunks)
        {
            if(chunkTmp.chunkIndex == chunkIndex)
                chunk = chunkTmp.chunkEntity;
        }
        if(chunk == Entity.Null) return;

        var players = playerNeedChunkLookup[chunk];

        tick.Add(2u);
        rpc.SetPlayer(networkID,tick);
        
        foreach(var player in players)
        {
            var connection = state.EntityManager.GetComponentData<PlayerSourceConnection>(player.playerEntity).value;
            RPCHelper.SendRpc(ecb,connection,rpc);
        }
    }



    public static void SendEventToClient<T>(EntityCommandBuffer ecb,int networkID,NetworkTick tick,Entity target)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        var rpc = new T();
        tick.Add(2u);
        rpc.SetPlayer(networkID,tick);
        RPCHelper.SendRpc(ecb,target,rpc);
    }
}
