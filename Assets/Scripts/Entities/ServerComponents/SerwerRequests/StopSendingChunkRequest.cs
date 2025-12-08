

using Unity.Entities;

public struct StopSendingChunkRequest : IComponentData
{
    public Entity player;
    public int chunk;    
}

