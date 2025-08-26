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
    [GhostField] public InventoryPosition position;
    [GhostField] public int ItemId;    
    [GhostField] public int Quantity; 
}

public struct InventoryPosition
{
    [GhostField] public int slotIndex;
    [GhostField] public byte container;
}
