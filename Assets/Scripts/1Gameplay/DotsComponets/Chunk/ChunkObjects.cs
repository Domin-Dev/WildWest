using Unity.Entities;

public struct ChunkObjects : IBufferElementData
{
    public Entity entity;
    public int ghostID;

    public ChunkObjects(Entity entity,int ghostID)
    {
        this.entity = entity;
        this.ghostID = ghostID;
    }
}