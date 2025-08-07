using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public struct MaxHitPoints : IComponentData
{
    public int value;
}

public struct CurrentHitPoints : IComponentData
{
    [GhostField] public int value;
}


[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct DamageBufferElement : IBufferElementData
{
    public int value;
}



public struct LastAction : IComponentData
{
    public NetworkTick tick;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted,OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public int value;
}

public struct DestroyOnTimer :  IComponentData
{ 
    public float value;
}

public struct DestroyAtTick : IComponentData
{
    [GhostField] public NetworkTick tick;
}

public struct DestroyEntityTag : IComponentData{ }


[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct CooldownTargetTick : ICommandData
{
   [SerializeField] public NetworkTick Tick { get; set; }
    [SerializeField] public NetworkTick ability { get; set; }
}


