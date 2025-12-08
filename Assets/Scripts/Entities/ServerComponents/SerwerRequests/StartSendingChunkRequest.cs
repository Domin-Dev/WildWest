

using Unity.Entities;

public struct StartSendingChunkRequest : IComponentData
{
    public Entity player;
    public int chunk;    
}

