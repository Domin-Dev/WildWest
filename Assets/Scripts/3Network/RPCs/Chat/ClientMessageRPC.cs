using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct ClientMessageRPC : IRpcCommand,IMessage
{
    public FixedString512Bytes message;
    public FixedString512Bytes Message
    {
        get => message;
        set => message = value;
    }
    public ushort ChunkIndex {get;set;}
    public ushort ChunkCount {get;set;}
    public uint MessageIndex {get;set;}
    public override string ToString()
    {
        return message.ToString();
    }
}
