
using Unity.Entities;
using Unity.Mathematics;

public struct NewParticles : IComponentData
{
    public Entity target;
    public float3 offset;
}
