using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct NewPlayerJoinRPC : IRpcCommand
{
    public FixedString64Bytes playerName;
}

public struct MapIsLoaded : IRpcCommand
{

}
