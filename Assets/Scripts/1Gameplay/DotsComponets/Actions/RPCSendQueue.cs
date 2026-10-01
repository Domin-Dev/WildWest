using Unity.Entities;
using Unity.NetCode;


public struct RPCSendQueue : IComponentData
{
    public NetworkTick tick;
    public Entity target;
}