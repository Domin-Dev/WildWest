using Unity.Entities;

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