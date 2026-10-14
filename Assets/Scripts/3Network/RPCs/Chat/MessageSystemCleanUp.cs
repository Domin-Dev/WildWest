using Unity.Entities;

public struct MessageCleanUp : IComponentData
{
    public uint CleanupPeriod;
    public uint MessageChunkLifetime;
}