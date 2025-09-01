using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct PlayerSaltRPC : IRpcCommand
{
    public FixedString128Bytes salt;
}