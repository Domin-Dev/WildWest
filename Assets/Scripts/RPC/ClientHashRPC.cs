using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct ClientHashRPC : IApprovalRpcCommand
{
    public FixedString128Bytes hash;
}
