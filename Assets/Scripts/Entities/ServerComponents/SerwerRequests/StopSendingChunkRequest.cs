

using Unity.Entities;

public struct StopSendingChunkRequest : IComponentData, IPriority
{
    public Entity player;
    public int chunk;

    public int priority;
    int IPriority.priority => priority;
}

