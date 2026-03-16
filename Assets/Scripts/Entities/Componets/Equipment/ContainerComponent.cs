using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


[GhostComponent]
public struct ContainerComponent : IComponentData
{
    [GhostField] public ContainerStats containerStats;

    public int containerIndex => containerStats.containerIndex;
    public MandatoryProperties mandatoryProperties => containerStats.mandatoryProperties;
    public int mandatoryData => containerStats.mandatoryData;
    public int capacity => containerStats.capacity;
    public byte waterResistance => containerStats.waterResistance;
    public ContainerType containerType => EQHelperClient.GetContainerType(containerIndex);
    public bool serverContainer => containerStats.serverContainer;

}

public struct PlayerContainer : IComponentData
{
    public Entity player;
}


public struct ServerContainer : IComponentData{}



[System.Serializable]
public struct ContainerStats
{
    public bool serverContainer;
    public int containerIndex;
    public MandatoryProperties mandatoryProperties;
    public int mandatoryData;
    public int capacity;
    public byte waterResistance;
}



public enum MandatoryProperties : byte
{
    none = 0,
    tag = 1,
    item = 2,
}