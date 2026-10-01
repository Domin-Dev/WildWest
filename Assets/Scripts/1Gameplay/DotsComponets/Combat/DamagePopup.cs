using Unity.Entities;
using Unity.Mathematics;

public struct DamagePopup : IComponentData
{
    public float3 startPosition;
    public int damageValue;
    public float elapsedTime;
    public float lifetime;
    public float3 moveDirection;
    public DamageTag damageTag;
}
