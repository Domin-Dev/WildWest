using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.OnlyPredictedClients)]
public struct Bullet : IComponentData
{
    [GhostField] public uint bulletID;
    public float speed;
    public float range;
    public int damage;
}