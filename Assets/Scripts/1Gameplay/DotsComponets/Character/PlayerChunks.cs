using Unity.Entities;

public struct PlayerChunks : IBufferElementData
{
    public int chunkIndex;
    public Entity chunkEntity;
    public double time;
}