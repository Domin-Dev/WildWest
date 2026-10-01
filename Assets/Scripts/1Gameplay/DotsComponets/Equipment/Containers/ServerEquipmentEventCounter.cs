using Unity.Entities;

public struct ServerEquipmentEventCounter : IComponentData,IIndexed
{
    public uint index;
    public uint GetIndex() { return index; }
}

