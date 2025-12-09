

using Unity.Entities;

public struct LoadChunkRequest : IComponentData, IPriority
{
    public Entity player;
    public int networkID;


    public int chunk;    
    public int priority;
    int IPriority.priority => priority;
}

