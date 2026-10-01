

using Unity.Entities;

public struct StartSendingChunkRequest : IComponentData, IPriority
{
    public Entity playerEntity;
    public int networkID;

    public int chunkIndex;
    public Entity chunkEntity;
    public int priority;
    
    int IPriority.priority => priority;

    public StartSendingChunkRequest(LoadChunkRequest loadChunkRequest, Entity chunkEntity)
    {
        this.playerEntity = loadChunkRequest.playerEntity;
        this.chunkIndex = loadChunkRequest.chunkIndex;
        this.networkID = loadChunkRequest.networkID;
        this.priority = loadChunkRequest.priority;
        this.chunkEntity = chunkEntity;
    }
}

