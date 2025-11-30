

using Unity.Entities;

public struct EQOnEquip : IComponentData
{
    public SlotPosition slotPosition;
    public Entity connection;
}



public struct EQOnEquipClient : IComponentData
{
    public SlotPosition slotPosition;
    public Entity container;
}
