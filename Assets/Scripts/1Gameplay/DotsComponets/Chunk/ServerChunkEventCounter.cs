using Unity.Entities;

public struct ServerChunkEventCounter : IComponentData,IIndexed
{
    public uint index;
    public uint GetIndex() { return index; }
}