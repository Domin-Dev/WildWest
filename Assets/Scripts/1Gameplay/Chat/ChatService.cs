using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public static class ChatService
{
    private static ushort _messageIndex = 0;
    private static ushort nextMessageIndex => ++_messageIndex;
 
    #region SendMessage
    public static void ClientSendsMessage(string messageText)
    {      
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        SendMessage<ClientMessageRPC>(ecb,messageText);
        ecb.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
        ecb.Dispose();
    }    
    public static void ServerSendsMessage(EntityCommandBuffer ecb, string messageText,Entity targetConnection)
    {      
        SendMessage<ServerMessageRPC>(ecb,messageText,new ServerMessageRPC(true),targetConnection);
    }
    public static void ServerSendsMessage(EntityCommandBuffer ecb, string messageText)
    {
        SendMessage<ServerMessageRPC>(ecb,messageText,new ServerMessageRPC(true));
    }
    public static void ServerSendsClientMessage(EntityCommandBuffer ecb,in FixedString128Bytes senderName,in ClientMessageRPC message)
    {      
        RPCService.SendRpc(ecb,new ServerMessageRPC(message,senderName,nextMessageIndex)); 
    }


    private static void SendMessage<T>(EntityCommandBuffer ecb, string messageText) where T : unmanaged,IRpcCommand,IMessage
    {
        SendMessageBase(ecb,messageText,new T(),rpc => RPCService.SendRpc(ecb,rpc));
    }    
    private static void SendMessage<T>(EntityCommandBuffer ecb, string messageText,in T message) where T : unmanaged,IRpcCommand,IMessage
    {
        SendMessageBase(ecb,messageText,message,rpc => RPCService.SendRpc(ecb,rpc));
    }
    private static void SendMessage<T>(EntityCommandBuffer ecb, string messageText,Entity targetConnection) where T : unmanaged,IRpcCommand,IMessage
    {
        SendMessageBase(ecb,messageText,new T(),rpc => RPCService.SendRpc(ecb,targetConnection,rpc));
    }  
    private static void SendMessage<T>(EntityCommandBuffer ecb, string messageText,in T message,Entity targetConnection) where T : unmanaged,IRpcCommand,IMessage
    {
        SendMessageBase(ecb,messageText,message,rpc => RPCService.SendRpc(ecb,targetConnection,rpc));
    }  
    private static void SendMessageBase<T>(EntityCommandBuffer ecb, string messageText,in T message,Action<T> sendRPC) where T : unmanaged,IRpcCommand,IMessage
    {
        var list = GetFixedString(messageText);
        if(list.Count == 0 || list.Count > ushort.MaxValue) return;  

        T clone = message;
        ushort chunkCount = (ushort)list.Count;
        clone.MessageIndex = nextMessageIndex;   
        clone.ChunkCount = chunkCount; 

        for(ushort i = 0; i < chunkCount; i++)
        {
            clone.ChunkIndex = i;
            clone.Message = list[i];
            UnityEngine.Debug.Log( list[i] + "test " + clone.Message);
            sendRPC.Invoke(clone);
        }
    }    
    #endregion
    #region Tools
    public static List<FixedString512Bytes> GetFixedString(string value)
    {
        var result = new List<FixedString512Bytes>();
        if (string.IsNullOrEmpty(value))
            return result;
        if(Encoding.UTF8.GetByteCount(value) <= FixedString512Bytes.UTF8MaxLengthInBytes)
        {
            result.Add(new FixedString512Bytes(value));
            return result;
        }
        
        var sb = new StringBuilder();
        int bytes = 0;
        for (int i = 0; i < value.Length; i++)
        {
            int charLength = char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]) ? 2 : 1;
            string character = value.Substring(i, charLength);
            int charBytes = Encoding.UTF8.GetByteCount(character);

            if (bytes + charBytes > FixedString512Bytes.UTF8MaxLengthInBytes)
            {
                result.Add(new FixedString512Bytes(sb.ToString()));
                sb.Clear();
                bytes = 0;
            }
            sb.Append(character);
            bytes += charBytes;
            if (charLength == 2)
                i++;
        }
        if (sb.Length > 0)
            result.Add(new FixedString512Bytes(sb.ToString()));
        return result;
    }
    public static string CombineMessageChunks(NativeArray<(ClientMessageRPC message,NetworkTick tick)> messageRPCs)
    {
        StringBuilder sb = new StringBuilder();
        foreach(var chunk in messageRPCs.OrderBy(x => x.message.ChunkIndex))
            sb.Append(chunk.message.ToString());
        return sb.ToString();
    }
    public static bool IsCommand(string value)
    {
        return value.Trim()[0] == '/';
    }
    #endregion
    #region Server
    private static void ProcessCommand(EntityManager entityManager,EntityCommandBuffer ecb, string command,Entity connectionEntity)
    {
        var commandList = DebugController.CommandList;
        string[] properties = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0, 1);
        List<CommandBase> hints = new List<CommandBase>();

        foreach (var item in commandList)
        {
            CommandBase commandBase = item as CommandBase;
            if (string.Compare(commandBase.commandId, properties[0], true) == 0)
            {
                string[] args = properties.Skip(1).ToArray();
                if (item.Validate(args))
                {
                    if(item.isAdminCommand)
                    {
                        if(entityManager.HasComponent<Admin>(connectionEntity))
                        {
                            var output = item.Invoke(args, ref ecb, connectionEntity);
                            if (!string.IsNullOrEmpty(output))
                                SendMessage<ServerMessageRPC>(ecb, output, connectionEntity);
                        }
                        else
                        {
                            SendMessage<ServerMessageRPC>(ecb, "You don't have permission!", connectionEntity);
                        }
                    }
                }      
            }
        }  
    }    
    public static void ProcessMessage(EntityManager entityManager,EntityCommandBuffer ecb,in FixedString128Bytes senderName,in ClientMessageRPC message, Entity senderEntity)
    {
        string textStr = message.ToString();
        UnityEngine.Debug.Log("command " + " " + textStr + " " + message.Message);
        if(IsCommand(textStr))
            ProcessCommand(entityManager,ecb,textStr.Trim(),senderEntity);
        else
            ServerSendsClientMessage(ecb,senderName,message); 
    }   
    public static void ProcessMessage(EntityManager entityManager,EntityCommandBuffer ecb,in FixedString128Bytes senderName,NativeArray< (ClientMessageRPC message,NetworkTick tick)> messages, Entity senderEntity)
    {
        string textStr = CombineMessageChunks(messages);
        if(IsCommand(textStr))
            ProcessCommand(entityManager,ecb,textStr.Trim(),senderEntity);
        else
            foreach(var message in messages)
            ServerSendsClientMessage(ecb,senderName,message.message); 
    }
    #endregion
}