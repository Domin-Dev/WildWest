

using Unity.Entities;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ChunkTiles : IBufferElementData
{
    [GhostField] public int tileID;
    [GhostField] public byte variant;
}
