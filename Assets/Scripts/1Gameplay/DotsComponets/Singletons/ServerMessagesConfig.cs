
using Unity.Entities;

public struct ServerMessagesConfig : IComponentData
{
    public uint CleanupPeriod;
    public uint MessageChunkLifetime;
}