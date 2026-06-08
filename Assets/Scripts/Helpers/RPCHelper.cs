using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
    public static void SendMessageToClients(EntityCommandBuffer ecb, string serverMessage)
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
    
    public static void CreateSerwerLocalEvent<T>(T rpc,EntityCommandBuffer ecb,Entity playerEntity,int networkID,NetworkTick tick,bool instantProcess = true) where T : unmanaged, IComponentData, ISetPlayer
    {
        var rpcEvent = ecb.CreateEntity();
        ecb.AddComponent(rpcEvent,new ServerEventData(tick));
        
        ecb.AddComponent<WaitForProcess>(rpcEvent);
        if(instantProcess)
            ecb.SetComponentEnabled<WaitForProcess>(rpcEvent,false);
        else
        {
            ecb.AppendToBuffer<FutureEventsForPlayer>(playerEntity,new FutureEventsForPlayer(){ entityEvent = rpcEvent});
        }

        tick.Add(1u); 
        rpc.SetPlayer(networkID,tick);
        ecb.AddComponent(rpcEvent,rpc);
        ecb.AddBuffer<SendEventToPlayers>(rpcEvent);
        ecb.AppendToBuffer(rpcEvent,new SendEventToPlayers() {connection = playerEntity });
    }
    public static bool SendEventsToClientsAndOwner<T>(ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID, Entity playerEntity, int chunkIndex, NetworkTick tick)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
       return SendEventsToClientsAndOwner(new T(),ref state,playerNeedChunkLookup,loadedChunks,ecb,networkID,playerEntity,chunkIndex,tick);
    }
    public static bool SendEventsToClients<T>(ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID,Entity playerEntity, int chunkIndex, NetworkTick tick)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
       return SendEventsToClients(new T(),ref state,playerNeedChunkLookup,loadedChunks,ecb,networkID,playerEntity,chunkIndex,tick);
    }
    public static bool SendEventsToClients<T>(T rpc,ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID,Entity playerEntity, int chunkIndex, NetworkTick tick, bool instantProcess = true)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        Entity chunk = Entity.Null; 
        foreach(var chunkTmp in loadedChunks)
        {
            if(chunkTmp.chunkIndex == chunkIndex)
                chunk = chunkTmp.chunkEntity;
        }
        if(chunk == Entity.Null) return false;

        var players = playerNeedChunkLookup[chunk];


        var rpcEvent = ecb.CreateEntity();
        ecb.AddComponent(rpcEvent,new ServerEventData(tick));
        ecb.AddComponent<WaitForProcess>(rpcEvent);
        if(instantProcess)
            ecb.SetComponentEnabled<WaitForProcess>(rpcEvent,false);
            else
        {
            ecb.AppendToBuffer<FutureEventsForPlayer>(playerEntity,new FutureEventsForPlayer(){ entityEvent = rpcEvent});
        }
        tick.Add(1u);
        
        rpc.SetPlayer(networkID,tick);
        ecb.AddComponent(rpcEvent,rpc);
        ecb.AddBuffer<SendEventToPlayers>(rpcEvent);
        ecb.AppendToBuffer(rpcEvent,new SendEventToPlayers() {connection = playerEntity });
        foreach(var player in players)
        {
            if(player.networkID != networkID)
            {
                var connection = state.EntityManager.GetComponentData<PlayerSourceConnection>(player.playerEntity).value;
                ecb.AppendToBuffer(rpcEvent,new SendEventToPlayers()
                {
                    connection = connection
                });
            }
        }
        return true;
    }
    public static bool SendEventsToClientsAndOwner<T>(T rpc,ref SystemState state,BufferLookup<PlayersNeedChunk> playerNeedChunkLookup,DynamicBuffer<LoadedChunks> loadedChunks,EntityCommandBuffer ecb,int networkID,Entity playerEntity, int chunkIndex, NetworkTick tick, bool instantProcess = true)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        Entity chunk = Entity.Null; 
        foreach(var chunkTmp in loadedChunks)
        {
            if(chunkTmp.chunkIndex == chunkIndex)
                chunk = chunkTmp.chunkEntity;
        }

        
        if(chunk == Entity.Null) return false;

        var players = playerNeedChunkLookup[chunk];


        var rpcEvent = ecb.CreateEntity();

        ecb.AddComponent(rpcEvent,new ServerEventData(tick));
        ecb.AddComponent<WaitForProcess>(rpcEvent);
        if(instantProcess)
            ecb.SetComponentEnabled<WaitForProcess>(rpcEvent,false);
        else
        {
            ecb.AppendToBuffer<FutureEventsForPlayer>(playerEntity,new FutureEventsForPlayer(){ entityEvent = rpcEvent});
        }

        tick.Add(1u);
        rpc.SetPlayer(networkID,tick);
        ecb.AddComponent(rpcEvent,rpc);
        ecb.AddBuffer<SendEventToPlayers>(rpcEvent);
        ecb.AppendToBuffer(rpcEvent,new SendEventToPlayers() {connection = playerEntity });

        foreach(var player in players)
        {
            var connection = state.EntityManager.GetComponentData<PlayerSourceConnection>(player.playerEntity).value;
            ecb.AppendToBuffer(rpcEvent,new SendEventToPlayers()
            {
                connection = connection
            });
        }
        return true;
    }
    public static void SendEventToClient<T>(EntityCommandBuffer ecb,int networkID,NetworkTick tick,Entity target)
    where T : unmanaged, IRpcCommand,ISetPlayer
    {
        var rpc = new T();
        tick.Add(1u);
        rpc.SetPlayer(networkID,tick);
        RPCHelper.SendRpc(ecb,target,rpc);
    }
}
