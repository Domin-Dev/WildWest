

using Unity.Entities;


public struct GhostChildren : IBufferElementData
{
    public Entity child;
    public int ghostID;
}

