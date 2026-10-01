using Unity.Entities;
using Unity.Mathematics;

public struct AnimationComponent : IComponentData
{
    public float elapsedTime;
    public bool hasStartPosition;
    public float playbackSpeed;
   


    public int itemID;
    public float3 startPosition;
    public quaternion startRotation;
    public float3 characterCenterPosition;
    public BodyPartType bodyPartType;
    public Entity player;
}
