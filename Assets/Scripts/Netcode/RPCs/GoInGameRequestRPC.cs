using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct GoInGameRequestRPC : IRpcCommand
{
    public FixedString64Bytes playerName;
    public CharacterLook characterLook;
}
