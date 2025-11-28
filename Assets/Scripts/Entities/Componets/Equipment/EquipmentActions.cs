
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;



public struct EquipmentEventData
{
    public int slot;
    public byte flags;
    public int value;

    public EquipmentEventData(int slot, byte flags, int value = 0)
    {
        this.slot = slot;
        this.flags = flags;
        this.value = value;
    }
}
public struct EquipmentEvent : IComponentData
{
    public EquipmentEventData data;
    public int networkID;
    public int containerIndex;
    public SlotPosition slotPosition => new SlotPosition(containerIndex,data.slot);

    public EquipmentEvent(EquipmentEventData data, int containerIndex, int networkID = 0)
    {
        this.data = data;
        this.networkID = networkID;
        this.containerIndex = containerIndex;
    }

    public void SetNetworkID(int networkID)
    {
        this.networkID = networkID;
    }
}

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
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
}

public struct PlayerContainers : IBufferElementData
{
    public Entity entity;
    public int index;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct EquipmentEventCounter : IInputComponentData
{
    [GhostField(Quantization = 0)] public uint index;

}
public struct ServerEquipmentEventCounter : IComponentData,IIndexed
{
    public uint index;
    public uint GetIndex() { return index; }
}

