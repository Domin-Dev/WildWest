

using Unity.Entities;

public struct EQOnEquip : IComponentData
{
    public InventorySlot oldInventorySlot;
    public SlotPosition slotPosition;
    public Entity player;
}



public struct EQOnEquipClient : IComponentData
{
    public SlotPosition slotPosition;
    public int ownerID;
}
