using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct AnimationFrames : IBufferElementData
{
    public bool processed;
    public PositionMode positionMode;
    public int frameIndex;

    public float duration;
    public float3 targetPosition;
    public quaternion targetRotation;
    public Entity targetEntity;

    public void Process(AnimationComponent animationComponent,Hands hands,LocalTransform localTransform)
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