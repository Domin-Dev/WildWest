
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerName : IComponentData
{
    public FixedString128Bytes name;
}

[System.Serializable]
public struct CharacterLook
{
    public int hairIndex;
    public int beardndex;
    public int faceDetailsIndex;

    public float3 skinColor;
    public float3 underwearColor;
    public float3 hairColor;
}

public struct LocalPlayerLook : IComponentData
{
   public CharacterLook characterLook;
}

public struct NewPlayerTag : IComponentData{}
