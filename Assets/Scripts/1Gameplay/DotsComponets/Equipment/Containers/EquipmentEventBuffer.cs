
using Unity.Entities;
using Unity.NetCode;

[GhostComponent]
public struct EquipmentEventBuffer : IBufferElementData, IIndexed
{
    [GhostField] public EquipmentEventData data;
    [GhostField] public uint index;
    public uint GetIndex() { return index; }

    public EquipmentEventBuffer(int slot, byte flags, int value = 0)
    {
        data = new EquipmentEventData()
        { 
            slot = slot,
            flags = flags,
            value = value
        };
        index = 0;
    }
   
    public EquipmentEventBuffer(EquipmentEventData data,uint index)
    {
        this.data = data;
        this.index = index;
    }

    // flags
    // 0 
    // 1 - update ItemSlot;
    //
}