using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct PlayerLeftRPC : IRpcCommand
{
    public FixedString128Bytes playerName;
    public byte ReasonCode;
    public long messageTime;
}