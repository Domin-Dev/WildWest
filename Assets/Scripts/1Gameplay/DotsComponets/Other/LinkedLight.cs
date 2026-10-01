using Unity.Entities;
using Unity.Mathematics;

public struct LinkedLight : IComponentData
{
    public float Intensity;
    public float InnerRadius;
    public float OuterRadius;
    public float FalloffIntensity;
    public float3 Offset;
}