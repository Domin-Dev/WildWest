

using Unity.Entities;

public struct StartSendingChunkRequest : IComponentData, IPriority
{
    public Entity playerEntity;
    public int networkID;

    public int chunkIndex;
    public Entity chunkEntity;
    public int priority;


    
    int IPriority.priority => priority;
}

