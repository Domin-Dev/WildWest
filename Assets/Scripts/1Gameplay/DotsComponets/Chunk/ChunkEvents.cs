using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ChunkEvents : IBufferElementData ,IIndexed
{
    [GhostField] public int chunk;
    [GhostField] public int2 tilePosition;
    [GhostField] public ChunkEventType flags;
    [GhostField] public uint index;

    public uint GetIndex()
    {
        return index;
    }
    // byte
    // 0 null
    // 1 readChunk Value.x = chunk index
}