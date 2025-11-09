using Unity.Entities;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)] 
public struct InventorySlot : IBufferElementData, IGetSlot
{
    [GhostField] public int slot;

    [GhostField] public int itemId;    
    [GhostField] public int quantity;
    [GhostField] public byte wetness; // 0% - 100%

    public override string ToString()
    {
        return $"Slot:{slot} ItemID:{itemId} Quantity:{quantity}";
    }

    public int GetSlot() { return slot; }
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