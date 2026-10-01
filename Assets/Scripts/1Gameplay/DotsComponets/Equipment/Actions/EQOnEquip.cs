

using Unity.Entities;

public struct EQOnEquip : IComponentData
{
    public InventorySlot oldInventorySlot;
    public SlotPosition slotPosition;
    public Entity player;
}

