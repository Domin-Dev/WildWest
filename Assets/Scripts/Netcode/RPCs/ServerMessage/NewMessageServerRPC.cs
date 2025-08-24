using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct NewMessageServerRPC : IRpcCommand
{
    public FixedString512Bytes message;
    public FixedString128Bytes sender;
    public long messageTime;
    public bool senderIsServer;
}