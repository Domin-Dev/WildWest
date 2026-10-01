using Unity.Entities;
using Unity.NetCode;

public struct DestroyAtTick : IComponentData
{
    [GhostField] public NetworkTick tick;
}