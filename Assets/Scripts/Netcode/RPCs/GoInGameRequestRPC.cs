using Unity.NetCode;
using UnityEngine;

public struct GoInGameRequestRPC : IRpcCommand
{
    public int value;
}
