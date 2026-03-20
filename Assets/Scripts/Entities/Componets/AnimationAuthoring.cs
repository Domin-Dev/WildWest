

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

            Transform parent = authoring.transform.parent;
            while(parent != null && parent.tag != "Player")
            {
                parent = parent.parent;
            }
            Entity playerEntitiy = GetEntity(parent.gameObject,TransformUsageFlags.Dynamic);


            AddComponent(entity, new AnimationComponent() 
            {
                 bodyPartType = authoring.bodyPartType,
                 elapsedTime = 0,
                 player = playerEntitiy
            });
            AddBuffer<AnimationFrames>(entity);
            AddBuffer<AnimationEvents>(entity);


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


    public Entity player;
}



public struct AnimationEvents : IBufferElementData
{
    public EventType eventType;
    public int id;
    public int frameIndex;
    public float3 position;
    public quaternion rotation;

    public bool relativeRotation;


}
public struct AnimationFrames : IBufferElementData
{
    public bool processed;
    public PositionMode positionMode;
    public int frameIndex;

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