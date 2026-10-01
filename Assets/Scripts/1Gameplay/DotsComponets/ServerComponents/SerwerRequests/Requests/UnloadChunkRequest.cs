

using Unity.Entities;

public struct UnloadChunkRequest : IComponentData, IPriority
{
    public int chunkIndex;    

    public Entity chunkEntity;
    public int priority;
    int IPriority.priority => priority;
}
