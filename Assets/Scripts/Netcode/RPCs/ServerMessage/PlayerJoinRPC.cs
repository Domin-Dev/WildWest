using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


public struct PlayerJoinRPC : IRpcCommand
{
    public FixedString128Bytes playerName;
    public long messageTime;
}
