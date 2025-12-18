

using Unity.Entities;

public struct LoadChunkRequest : IComponentData, IPriority
{
    public Entity playerEntity;
    public int networkID;


    public int chunkIndex;    
    public int priority;
    int IPriority.priority => priority;
}

