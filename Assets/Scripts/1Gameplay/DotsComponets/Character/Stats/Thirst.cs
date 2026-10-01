using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct Thirst : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}