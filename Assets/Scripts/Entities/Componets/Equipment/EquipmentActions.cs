
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;





public static class EquipementEventFlags
{
    public const int UpdateSlot = 1;
    public const int ClearContainer = 2;
    public const int ClearAllContainers = 3;
    public const int UpdateWetness = 4;
    public const int UpdateWeaponMagazine = 5;
    public const int UpdateOutfit = 6;
    public const int BrokenItem = 7;
}

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

    public EquipmentEvent(byte flagEvent)
    {
        this.data = new EquipmentEventData(){ flags = flagEvent};
        this.networkID = 0;
        this.containerIndex = 0;
    }

    public void SetNetworkID(int networkID)
    {
        this.networkID = networkID;
    }
}

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

public struct EntityContainers : IBufferElementData
{
   [GhostField] public Entity entity;
   [GhostField] public int index;
}


public struct WorldItemEntity : ICleanupBufferElementData, IGetSlot
{
    public int slot;
    public Entity worldItem;

    public int GetSlot()
    {
        return slot;
    }

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
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

