using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct ClientHashRPC : IRpcCommand
{
    public FixedString128Bytes hash;
}
