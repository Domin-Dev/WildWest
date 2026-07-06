


using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class KeyFrame
{
    [SerializeField] private BodyPartType bodyPartType;
    [SerializeField] private PositionMode positionMode;
    [SerializeField] private float3 rotate;
    [SerializeField] private float3 move;
    [SerializeField] private float duration;
    [SerializeField] private List<AnimationEvent> events;

    public BodyPartType BodyPartType => bodyPartType;
    public float Duration => duration;
    public float3 Move => move;
    public quaternion Rotation => quaternion.Euler(math.radians(rotate)); 
    public List<AnimationEvent> Events => events;


    public AnimationFrames GetAnimationFrame(int frameIndex)
    {
        return new AnimationFrames()
        {
            duration = Duration,
            targetRotation = Rotation,
            targetPosition = Move,
            positionMode = positionMode,
            processed = false,
            frameIndex = frameIndex
        };
    }
}


[System.Serializable]
public class AnimationEvent
{
    public EventType EventType;
    public float3 position;
    public float3 rotation;
    public bool relativeRotation;

    public quaternion Rotation => quaternion.Euler(math.radians(rotation)); 

    public IndexType indexType;
    public int id;

    public AnimationEvents GetEvent(int frameIndex,int[] args)
    {
        int index = id;
        if(indexType == IndexType.ArgumentID)
        {
            if(args != null && args.Length > id && id >= 0)
                index = args[id];
            else
                index = -1;
        }
     
        return new AnimationEvents()
        {
            eventType = EventType,
            indexType = indexType,
            id = index,
            frameIndex = frameIndex,
            position = position,
            rotation = Rotation,
            relativeRotation = relativeRotation
        };
    }
}



public enum IndexType : byte
{
    AssetID = 0,
    ArgumentID = 1,
}
public enum BodyPartType :  byte
{
    MainHand,
    SideHand
}
public enum EventType :  byte
{
    Sound,
    SpawnParticle,
    SpawnParticleAtAimPoint,
    SpawnParticleAtReloadPoint,
    SpawnParticleAtPointer,
    ChangeItemSprite,
    ChangeSpriteInSideHand
}
public enum PositionMode :  byte
{
    MoveLocal = 0,
    SetLocal = 1,
    MoveRelativeToStart = 2,
    MoveRelativeToReloadPoint = 3,
}


