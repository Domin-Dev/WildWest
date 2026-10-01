
using Unity.Entities;

public struct EQOnEquipClient : IComponentData
{
    public SlotPosition slotPosition;
    public int ownerID;
}
