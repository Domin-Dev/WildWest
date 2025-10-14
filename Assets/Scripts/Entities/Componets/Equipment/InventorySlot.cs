using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)] 
public struct InventorySlot : IBufferElementData
{
    [GhostField] public int slot;
    [GhostField] public int ItemId;    
    [GhostField] public int quantity; 
}

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ContainerComponent : IComponentData
{
    [GhostField] public byte containerIndex;
    [GhostField] public int capacity;

    [GhostField] public MandatoryProperties mandatoryProperties; 
    [GhostField] public int mandatoryData;
}



public enum MandatoryProperties : byte
{ 
    none = 0,
    tag = 1,
    item = 2,
}
public struct ContainerLoaded : IComponentData
{
}