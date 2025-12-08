

using Unity.Entities;

public struct LoadChunkRequest : IComponentData
{
    public Entity player;
    public int chunk;    
}

