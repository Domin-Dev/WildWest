using Unity.Entities;
using Unity.NetCode;

public struct DestroyAtTick : IComponentData
{
    public NetworkTick tick;
}