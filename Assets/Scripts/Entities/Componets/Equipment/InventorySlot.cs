using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)] 
public struct InventorySlot : IBufferElementData
{
    [GhostField] public InventoryPosition position;
    [GhostField] public int ItemId;    
    [GhostField] public int quantity; 
}

public struct InventoryPosition
{
    [GhostField] public int slotIndex;
    [GhostField] public byte container;
}

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ContainerComponent : IComponentData
{
    [GhostField] public byte containerIndex;
    [GhostField] public int capacity;

    [GhostField] public MandatoryProperties mandatoryProperties; 
    [GhostField] public int mandatoryData;
}

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct Equipment : IBufferElementData
{
}

public struct ContainerLoaded : IComponentData
{
}


public enum MandatoryProperties : byte
{ 
    none = 0,
    tag = 1,
    item = 2,
}
