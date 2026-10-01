using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


[GhostComponent]
public struct ContainerComponent : IComponentData
{
    [GhostField] public ContainerStats containerStats;
    [GhostField] public int parentContainerIndex;


    public int containerIndex => containerStats.containerIndex;
    public MandatoryProperties mandatoryProperties => containerStats.mandatoryProperties;
    public int mandatoryData => containerStats.mandatoryData;
    public int capacity => containerStats.capacity;
    public byte waterResistance => containerStats.waterResistance;
    public ContainerType containerType => EQHelperClient.GetContainerType(containerIndex);
    public bool serverContainer => containerStats.serverContainer;
    public bool publicContainer => containerStats.publicContainer;
}




