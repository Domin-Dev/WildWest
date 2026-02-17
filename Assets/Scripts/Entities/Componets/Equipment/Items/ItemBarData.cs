using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent] 
public struct ItemBarData : IBufferElementData, IGetSlot
{
    [GhostField] public int slot;
    [GhostField] public float value;
    [GhostField] public float maxValue;

    public ItemBarData(BarDataSave save,int slot)
    {
        this.slot = slot;
        this.value = save.value;
        this.maxValue = save.maxValue;
    }

    public override string ToString()
    {
        return $"Slot:{slot} Value:[{value}/{maxValue}]";
    }

    public int GetSlot()
    {
        return slot;
    }
}


