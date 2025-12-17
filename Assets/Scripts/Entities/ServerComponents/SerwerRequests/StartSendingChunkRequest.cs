

using Unity.Entities;

public struct StartSendingChunkRequest : IComponentData, IPriority
{
    public Entity player;
    public int chunk;

    public int priority;
    int IPriority.priority => priority;
}

