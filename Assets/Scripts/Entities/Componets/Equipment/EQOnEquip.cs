

using Unity.Entities;

public struct EQOnEquip : IComponentData
{
    public SlotPosition slotPosition;
    public Entity connection;
}
