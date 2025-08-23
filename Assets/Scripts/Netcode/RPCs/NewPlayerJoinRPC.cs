using Unity.Collections;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public struct NewPlayerJoinRPC : IRpcCommand
{
    public FixedString128Bytes playerName;
}
public struct PlayerVerificationRPC: IRpcCommand
{
    public FixedString128Bytes playerName;
}
public struct AnswerPlayerVerificationRPC : IRpcCommand
{
    public CharacterLook characterLook;
    public bool playerDataIsOnServer;
}
public struct MapIsLoaded : IRpcCommand
{

}
