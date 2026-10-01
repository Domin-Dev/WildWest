using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct AimRotation : IComponentData
{
    [GhostField] public float angle;
} 
