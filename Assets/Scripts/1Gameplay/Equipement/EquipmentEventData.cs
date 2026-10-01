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