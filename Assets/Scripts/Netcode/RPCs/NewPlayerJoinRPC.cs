using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct NewPlayerJoinRPC : IRpcCommand
{
    public FixedString128Bytes playerName;
}

public struct MapIsLoaded : IRpcCommand
{

}
