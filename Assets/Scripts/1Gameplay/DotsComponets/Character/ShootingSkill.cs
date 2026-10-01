using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients,OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ShootingSkill : IComponentData
{
    public byte Value;
}

