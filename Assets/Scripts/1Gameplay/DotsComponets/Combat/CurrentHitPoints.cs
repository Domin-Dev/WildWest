using Unity.Entities;
using Unity.NetCode;

public struct CurrentHitPoints : IComponentData
{
    [GhostField] public int value;
}
