

using Unity.Entities;

public struct UpdateChunkTimestampRequest : IComponentData
{
    public Entity chunkEntity;
    public int chunkIndex;
    public Entity playerEntity;
}

