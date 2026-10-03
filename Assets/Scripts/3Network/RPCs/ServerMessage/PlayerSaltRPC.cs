using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct PlayerSaltRPC : IApprovalRpcCommand
{
    public FixedString128Bytes salt;
}