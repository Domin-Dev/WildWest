using Unity.Entities;
using Unity.NetCode;
public struct ExecuteAtTick : IComponentData
{
    public NetworkTick tick;
}