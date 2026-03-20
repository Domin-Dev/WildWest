

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



public class AnimationAuthoring : MonoBehaviour
{
    public BodyPartType bodyPartType;
    public class Baker : Baker<AnimationAuthoring>
    {
        public override void Bake(AnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new AnimationComponent() 
            {
                 bodyPartType = authoring.bodyPartType,
                 elapsedTime = 0,
            });
            AddBuffer<AnimationFrames>(entity);
            AddComponent<AnimationIsPaused>(entity);
            SetComponentEnabled<AnimationIsPaused>(entity,true);
        }
    }
}


public struct AnimationIsPaused : IComponentData, IEnableableComponent {}
public struct AnimationComponent : IComponentData
{
    public float elapsedTime;
    public bool hasStartPosition;

    public float3 startPosition;
    public quaternion startRotation;
    public BodyPartType bodyPartType;
}

public struct AnimationFrames : IBufferElementData
{
    public bool processed;
    public PositionMode positionMode;

    public float duration;
    public float3 targetPosition;
    public quaternion targetRotation;


    public void Process(AnimationComponent animationComponent, LocalTransform  localTransform)
    {
        processed = true;

        if(positionMode == PositionMode.Local)
        {
            targetRotation = math.normalize(math.mul(targetRotation, localTransform.Rotation));
            targetPosition = localTransform.Position + targetPosition;
        }
        else if(positionMode == PositionMode.RelativeToStart)
        {
            targetRotation = math.normalize(math.mul(targetRotation, animationComponent.startRotation));
            targetPosition = animationComponent.startPosition + targetPosition;
        }
    }                   
}