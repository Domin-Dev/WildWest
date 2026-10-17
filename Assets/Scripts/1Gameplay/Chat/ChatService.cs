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
    public static void ServerSendsClientMessage(EntityCommandBuffer ecb,in FixedString128Bytes senderName,NativeArray<(ClientMessageRPC message,NetworkTick tick)> messages)
    {  
        var index = nextMessageIndex;
        foreach(var message in messages)  
            RPCService.SendRpc(ecb,new ServerMessageRPC(message.message,senderName,index)); 
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
    public static string CombineMessageChunks<T>(NativeArray<(T message,NetworkTick tick)> messageRPCs) where T : unmanaged,IRpcCommand,IMessage
    {
        StringBuilder sb = new StringBuilder();
        foreach(var chunk in messageRPCs.OrderBy(x => x.message.ChunkIndex))
            sb.Append(chunk.message.Message.ToString());
        return sb.ToString();
    }
    public static bool IsCommand(string value)
    {
        if(value == null) return false;
        var temp = value.Trim();
        return temp.Length > 0 && temp[0] == '/';
    }
    #endregion
    #region ServerProcess
    private static void ServerProcessCommand(EntityManager entityManager,EntityCommandBuffer ecb, string commandText,Entity connectionEntity)
    {
        var commandList = DebugController.Commands;
        string[] properties = commandText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0, 1);
        List<CommandBase> hints = new List<CommandBase>();

        if (commandList.TryGetValue(properties[0].ToLower(),out var list))
        {
            string[] args = properties.Skip(1).ToArray();
            foreach(var command in list)
            {
                if (command.Validate(args) && command.isAdminCommand)
                {
                    if(entityManager.HasComponent<Admin>(connectionEntity))
                    {
                        var output = command.Invoke(args, ref ecb, connectionEntity);
                        if (!string.IsNullOrEmpty(output))
                            SendMessage<ServerMessageRPC>(ecb, output, connectionEntity);
                        return;
                    }
                    else
                    {
                        SendMessage<ServerMessageRPC>(ecb, "You don't have permission!", connectionEntity);
                        return;
                    }
                }   
            }  
        }
        
    }       
    public static void ServerProcessMessage(EntityManager entityManager,EntityCommandBuffer ecb,in FixedString128Bytes senderName,in ClientMessageRPC message, Entity senderEntity)
    {
        string textStr = message.ToString();
        if(IsCommand(textStr))
            ServerProcessCommand(entityManager,ecb,textStr.Trim(),senderEntity);
        else
            ServerSendsClientMessage(ecb,senderName,message); 
    }   
    public static void ServerProcessMessage(EntityManager entityManager,EntityCommandBuffer ecb,in FixedString128Bytes senderName,NativeArray< (ClientMessageRPC message,NetworkTick tick)> messages, Entity senderEntity)
    {
        var first = messages.First(x => x.message.ChunkIndex == 0);
        if(IsCommand(first.message.Message.ToString()))
        {
            string textStr = CombineMessageChunks(messages);
            ServerProcessCommand(entityManager,ecb,textStr.Trim(),senderEntity);
        }
        else
            ServerSendsClientMessage(ecb,senderName,messages);
    }

    #endregion
    #region ClientProcess
    public static void ClientProcessMessage(in ServerMessageRPC message)
    {
        if (message.senderIsServer)
            ChatManager.instance.PrintServerMessage(message.messageTime, message.Message.ToString());
        else
            ChatManager.instance.PrintPlayerMessage(message.messageTime, message.Message.ToString(), message.sender.ToString());
    }   
    public static void ClientProcessMessage(NativeArray<(ServerMessageRPC message,NetworkTick tick)> messages)
    {
        if(messages.Length == 0) return; 
        ServerMessageRPC message = messages[0].message;
        string textStr = CombineMessageChunks(messages);

        if (message.senderIsServer)
            ChatManager.instance.PrintServerMessage(message.messageTime, textStr);
        else
            ChatManager.instance.PrintPlayerMessage(message.messageTime, textStr, message.sender.ToString());
    }

    #endregion
}