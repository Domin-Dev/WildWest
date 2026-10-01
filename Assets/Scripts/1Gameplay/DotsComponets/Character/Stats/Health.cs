using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct Health : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}