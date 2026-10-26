using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;

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
    public static bool TrySplitCommand(string value,out string commandID,out string[] arguments)
    {
        if(IsCommand(value))
        {
            string[] command = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            commandID = command[0].Substring(1).ToLower();
            arguments = command.Skip(1).ToArray();
            return true;
        }
        else
        {
            commandID = null;
            arguments = null;
            return false;
        }
    }
    public static bool HasWhitespaceBeforeCaret(string value,int caretPosition)
    {
        value = value.TrimStart();
        for (int i = 0; i < caretPosition && i < value.Length; i++)
        {
            if (char.IsWhiteSpace(value[i]))
                return true;         
        }
        return false;
    }     
    public static int GetWhitespaceEnd(string value,int caretPosition)
    {
        int i = value.Length - 1; 
        while (i > caretPosition && char.IsWhiteSpace(value[i]))
            i--;
        return i + 1;
    } 
    public static int GetArgumentIndex(string value,int caretPosition)
    {
        int counter = 0;
        bool previous = true;
        for (int i = 0; i < caretPosition && i < value.Length; i++)
        {
            bool current = char.IsWhiteSpace(value[i]);
            if(!previous && current)
                counter++;
            previous = current;
        }
        return counter - 1;
    }
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
    public static void PrintCommands(params CommandBase[] commandBase)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < commandBase.Length; i++)
        {
            sb.Append($"/{commandBase[i].CommandId.GetColorfulString(ChatConfig.Instance.HighlightColor)} {commandBase[i].CommandFormat}");
            if(i != commandBase.Length - 1) sb.Append("\n");
        }
        if(sb.Length > 0) Chat.Instance.Print(sb.ToString());
    }
    #endregion
    #region ServerProcess
    private static void ExecuteServerCommand(EntityManager entityManager,EntityCommandBuffer ecb, string commandText,Entity connectionEntity)
    {
        if(TrySplitCommand(commandText,out var commandID,out var arguments))
        {
            var commandList = DebugController.Commands;
            if (commandList.TryGetValue(commandID,out var list))
            {
                foreach(var command in list)
                {
                    if (command.Validate(arguments) && command.IsAdminCommand)
                    {
                        if(entityManager.HasComponent<Admin>(connectionEntity))
                        {
                            var output = command.Invoke(arguments, ref ecb, connectionEntity);
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
    }       
    public static void ServerProcessMessage(EntityManager entityManager,EntityCommandBuffer ecb,in FixedString128Bytes senderName,in ClientMessageRPC message, Entity senderEntity)
    {
        string textStr = message.ToString();
        if(IsCommand(textStr))
            ExecuteServerCommand(entityManager,ecb,textStr.Trim(),senderEntity);
        else
            ServerSendsClientMessage(ecb,senderName,message); 
    }   
    public static void ServerProcessMessage(EntityManager entityManager,EntityCommandBuffer ecb,in FixedString128Bytes senderName,NativeArray< (ClientMessageRPC message,NetworkTick tick)> messages, Entity senderEntity)
    {
        var first = messages.First(x => x.message.ChunkIndex == 0);
        if(IsCommand(first.message.Message.ToString()))
        {
            string textStr = CombineMessageChunks(messages);
            ExecuteServerCommand(entityManager,ecb,textStr.Trim(),senderEntity);
        }
        else
            ServerSendsClientMessage(ecb,senderName,messages);
    }

    #endregion
    #region ClientProcess
    public static void ClientProcessMessage(in ServerMessageRPC message)
    {
        if (message.senderIsServer)
            Chat.Instance.PrintServerMessage(message.messageTime, message.Message.ToString());
        else
            Chat.Instance.PrintPlayerMessage(message.messageTime, message.Message.ToString(), message.sender.ToString());
    }   
    public static void ClientProcessMessage(NativeArray<(ServerMessageRPC message,NetworkTick tick)> messages)
    {
        if(messages.Length == 0) return; 
        ServerMessageRPC message = messages[0].message;
        string textStr = CombineMessageChunks(messages);

        if (message.senderIsServer)
            Chat.Instance.PrintServerMessage(message.messageTime, textStr);
        else
            Chat.Instance.PrintPlayerMessage(message.messageTime, textStr, message.sender.ToString());
    }
    public static bool TryExecuteLocalCommand(string value)
    {
        if(TrySplitCommand(value,out var commandID,out var arguments))
        {
            List<CommandBase> hints = new List<CommandBase>();           
            if (DebugController.Commands.TryGetValue(commandID.ToLower(),out var list))
            {
                foreach(var command in list)
                {               
                    if (command.Validate(arguments))
                    {
                        if (!command.IsServerCommand && !command.IsAdminCommand)
                        {
                            var ecb = new EntityCommandBuffer(Allocator.Temp);
                            var output = command.Invoke(arguments,ref ecb,Entity.Null);
                            ecb.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
                            ecb.Dispose();
                            if (!string.IsNullOrEmpty(output))
                                Chat.Instance.Print(output);
                            return true;
                        }
                        else
                            return false;
                    }
                    else 
                        hints.Add(command);
                }
            }
            if (hints.Count == 0)
                Chat.Instance.Print($"{"Incorrect command:".GetColorfulString(ChatConfig.Instance.WarningColor)} <noparse>{value}</noparse>");
            PrintCommands(hints.ToArray());
            return true;
        }
        else
            return false;
    }
    #endregion
}