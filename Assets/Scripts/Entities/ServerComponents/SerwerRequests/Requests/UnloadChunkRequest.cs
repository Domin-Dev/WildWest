

using Unity.Entities;

public struct UnloadChunkRequest : IComponentData, IPriority
{
    public Entity playerEntity;
    public int networkID;


    public int chunkIndex;    
    public int priority;
    int IPriority.priority => priority;

    public UnloadChunkRequest(StartSendingChunkRequest request)
    {
        this.playerEntity = request.playerEntity;
        this.networkID = request.networkID;
        this.chunkIndex = request.chunkIndex;
        this.priority = request.priority;
    }
}

