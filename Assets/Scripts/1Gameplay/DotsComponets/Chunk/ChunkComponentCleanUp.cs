using Unity.Entities;

public struct ChunkComponentCleanUp : ICleanupComponentData
{
    public int chunkIndex;
}
