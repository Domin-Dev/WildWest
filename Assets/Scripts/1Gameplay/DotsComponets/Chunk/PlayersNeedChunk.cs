using Unity.Entities;

public struct PlayersNeedChunk : IBufferElementData
{
    public Entity playerEntity;
    public int networkID;
}