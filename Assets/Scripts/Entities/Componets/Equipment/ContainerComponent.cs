using Unity.Entities;
using Unity.NetCode;


[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ContainerComponent : IComponentData
{
    [GhostField] public byte containerIndex;

    [GhostField] public MandatoryProperties mandatoryProperties;
    [GhostField] public int mandatoryData;
    
    [GhostField] public int capacity;
    [GhostField] public byte waterResistance;
}

public enum MandatoryProperties : byte
{
    none = 0,
    tag = 1,
    item = 2,
}