
using Unity.Entities;
using Unity.NetCode;

public struct Cooldown : IComponentData
{
    public NetworkTick cooldownTick;
    public NetworkTick startCooldown;
}