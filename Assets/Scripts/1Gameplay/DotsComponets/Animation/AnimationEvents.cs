using Unity.Entities;
using Unity.Mathematics;

public struct AnimationEvents : IBufferElementData
{
    public EventType eventType;
    public IndexType indexType;
    public int id;
    public int frameIndex;

    public float3 position;
    public quaternion rotation;

    public bool relativeRotation;
}