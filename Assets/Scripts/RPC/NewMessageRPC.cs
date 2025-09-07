using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct NewMessageRPC : IRpcCommand
{
    public FixedString512Bytes message;
}