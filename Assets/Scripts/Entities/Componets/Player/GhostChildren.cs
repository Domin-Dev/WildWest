

using Unity.Entities;


public struct GhostChildren : IBufferElementData
{
    public Entity child;
    public int ghostID;
}

public struct SynchronizeRelevancyWithParent : IComponentData
{
    public Entity parent;
}
