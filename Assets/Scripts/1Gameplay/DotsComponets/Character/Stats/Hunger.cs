using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct Hunger : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}