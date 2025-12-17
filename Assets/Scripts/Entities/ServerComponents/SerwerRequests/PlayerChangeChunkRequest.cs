

using Unity.Entities;

public struct PlayerChangeChunkRequest : IComponentData
{
    public Entity player;
    public int newChunk;
    public int lastChunk;
}

