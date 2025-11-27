using Unity.Entities;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)] 
public struct ItemBarData : IBufferElementData, IGetSlot
{
    [GhostField] public int slot;
    [GhostField] public float value;
    [GhostField] public float maxValue;

    public override string ToString()
    {
        return $"Slot:{slot} Value:[{value}/{maxValue}]";
    }

    public int GetSlot()
    {
        return slot;
    }
}


