

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
    public float playbackSpeed;
   


    public int itemID;
    public float3 startPosition;
    public quaternion startRotation;
    public float3 characterCenterPosition;
    public BodyPartType bodyPartType;
    public Entity player;
}





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
public struct AnimationFrames : IBufferElementData
{
    public bool processed;
    public PositionMode positionMode;
    public int frameIndex;

    public float duration;
    public float3 targetPosition;
    public quaternion targetRotation;
    public Entity targetEntity;

    public void Process(AnimationComponent animationComponent,Hands hands,LocalTransform  localTransform)
    {
        processed = true;
        targetEntity = Entity.Null;

        switch(positionMode)
        {
            case PositionMode.MoveLocal:
                targetRotation = math.normalize(math.mul(targetRotation, localTransform.Rotation));
                targetPosition = localTransform.Position + targetPosition;
                break;
            case PositionMode.SetLocal:
                targetRotation = math.normalize(targetRotation);
                if(animationComponent.bodyPartType == BodyPartType.SideHand && hands.twoHanded)
                    targetEntity = hands.side;
                else
                    targetEntity = hands.main;
                break;
            case PositionMode.MoveRelativeToStart:
                targetRotation = math.normalize(math.mul(targetRotation, animationComponent.startRotation));
                targetPosition = animationComponent.startPosition + targetPosition;
                break;
            case PositionMode.MoveRelativeToReloadPoint:
                targetRotation = math.normalize(targetRotation);
                targetEntity = hands.reloadPoint;
                break;
        }

    }                   


    public void GetTargetValues(ref SystemState state,Entity animatingObjects ,out float3 targetPos,out quaternion targetRot)
    {
        if(targetEntity == Entity.Null)
        {
            targetPos = targetPosition;
            targetRot = targetRotation;
        }
        else
        {
            LocalToWorld targetLocalToWorld = state.EntityManager.GetComponentData<LocalToWorld>(targetEntity);
            float3 offset = targetPosition;
            if(positionMode != PositionMode.SetLocal)
                offset = math.mul(targetLocalToWorld.Rotation,targetPosition);

            float3 targetWorldPos = targetLocalToWorld.Position + offset;

            if (state.EntityManager.HasComponent<Parent>(animatingObjects))
            {
                var parent = state.EntityManager.GetComponentData<Parent>(animatingObjects).Value;
                float4x4 parentWorldToLocal = math.inverse(
                    state.EntityManager.GetComponentData<LocalToWorld>(parent).Value
                );   
                float3 localPos = math.transform(parentWorldToLocal, targetWorldPos);

                localPos.z = targetPosition.z;
                targetPos = localPos;
            }
            else
                targetPos = targetWorldPos;

            targetRot = targetRotation;
        }
    } 

}