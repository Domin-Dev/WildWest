using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ChunkComponent : IComponentData
{
    [GhostField] public int chunkIndex;
    [GhostField] public float2 worldPos;
    [GhostField] public int regionIndex;
}


