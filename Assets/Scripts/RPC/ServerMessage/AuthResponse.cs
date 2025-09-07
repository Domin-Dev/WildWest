using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct AuthResponse : IApprovalRpcCommand
{
    public bool success;
}