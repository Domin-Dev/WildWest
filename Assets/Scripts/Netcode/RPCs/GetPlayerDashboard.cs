using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct GoInGameRequestRPC : IRpcCommand
{
    public FixedString128Bytes playerName;
    public CharacterLook characterLook;
}
