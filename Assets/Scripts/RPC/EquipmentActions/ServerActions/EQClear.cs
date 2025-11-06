using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQClear : IComponentData
{
    // index < 0 => clear all containers
    public int containerIndex;
    public Entity networkEntity;
}