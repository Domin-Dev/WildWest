using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct ServerMessageRPC : IRpcCommand, IMessage
{
    public FixedString128Bytes sender;
    public FixedString512Bytes message;
    public FixedString512Bytes Message
    {
        get => message;
        set => message = value;
    }
   
    public long messageTime;
    public bool senderIsServer;
    
    public ushort ChunkIndex {get;set;}
    public ushort ChunkCount {get;set;}
    public uint MessageIndex {get;set;}
    
    public ServerMessageRPC(bool senderIsServer)
    {
        this.message = "";
        this.sender = "Server";
        this.messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.senderIsServer = senderIsServer;

        this.ChunkIndex = 0;
        this.ChunkCount = 1;
        this.MessageIndex = 0;
    }
    public ServerMessageRPC(in FixedString512Bytes serverMessage)
    {
        this.message = serverMessage;
        this.sender = "Server";
        this.messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.senderIsServer = true;

        this.ChunkIndex = 0;
        this.ChunkCount = 1;
        this.MessageIndex = 0;
    }
    public ServerMessageRPC(in IMessage serverMessage,in FixedString128Bytes senderName,uint messageIndex)
    {
        this.message = serverMessage.Message;
        this.sender = senderName;
        this.messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.senderIsServer = false;

        this.ChunkCount = serverMessage.ChunkCount;
        this.ChunkIndex = serverMessage.ChunkIndex;
        this.MessageIndex = messageIndex;
    }
    public override string ToString()
    {
        return message.ToString();
    }
}