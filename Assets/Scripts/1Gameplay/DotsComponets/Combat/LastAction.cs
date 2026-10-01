using Unity.Entities;
using Unity.NetCode;

public struct LastAction : IComponentData
{
    public NetworkTick tick;
}