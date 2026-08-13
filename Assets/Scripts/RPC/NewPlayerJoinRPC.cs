using Unity.Collections;
using Unity.Mathematics;
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
public struct StartDataRPC : IRpcCommand
{
    public MapSetUp mapSetUp;
    public CurrentTime currentTime;
}

public struct MapSetUp
{
    public int mapSizeInRegions;
    public int regionSizeInChunks;
    public int chunkSizeInTiles;
    public float tileSize;
    public float2 mapOffset;
    public int seed;
}