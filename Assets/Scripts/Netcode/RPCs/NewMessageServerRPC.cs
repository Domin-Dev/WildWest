using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct NewMessageServerRPC : IRpcCommand
{
    public FixedString512Bytes message;
    public FixedString64Bytes sender;
    public long messageTime;
}