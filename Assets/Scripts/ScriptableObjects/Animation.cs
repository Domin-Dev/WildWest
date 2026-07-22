


using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

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
    
    [ShowIf("hasPostion")]
    [AllowNesting]
    public float3 position;
    
    [ShowIf("hasPostion")]
    [AllowNesting]
    public float3 rotation;
    
    [ShowIf("hasPostion")]
    [AllowNesting]
    public bool relativeRotation;

    [ShowIf("hasID")]
    [AllowNesting]
    public IndexType indexType;
    
    [ShowIf("hasID")]
    [AllowNesting]
    public int id;

    private bool hasPostion => EventTypeProperties.EventTypeNeedPosition(EventType);
    private bool hasID => EventTypeProperties.EventTypeNeedID(EventType);
    public quaternion Rotation => quaternion.Euler(math.radians(rotation)); 
    
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


[System.Serializable]
public class AnimationEventTab
{
    [SerializeField] private AnimationEventArg[] Events;
    public AnimationEvent[] GetAnimationEvents(Vector2[] points)
    {
        var animationEvents = new AnimationEvent[Events.Length];
        for(int i = 0; i < Events.Length;i++)
        {
            var e = Events[i];
            if(e.PointPosition >= 0)
            {
                float2 f = points[e.PointPosition];
                e.Event.position = new float3(f,f.y);
            }
            animationEvents[i] = e.Event;
        }  
        return animationEvents;
    }

}

[System.Serializable]
public class AnimationEventArg
{
    public AnimationEvent Event;
    public int PointPosition;
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
    ChangeSpriteInSideHand,
    RunReceivedEvents
}
public static class EventTypeProperties
{
    private static readonly EventType[] _needPosition = new []
    {
        EventType.SpawnParticle,
        EventType.SpawnParticleAtAimPoint,
        EventType.SpawnParticleAtReloadPoint,
        EventType.SpawnParticleAtPointer,
    };    
    
    private static readonly EventType[] _needID = new []
    {
        EventType.Sound,
        EventType.SpawnParticle,
        EventType.SpawnParticleAtAimPoint,
        EventType.SpawnParticleAtReloadPoint,
        EventType.SpawnParticleAtPointer,
        EventType.ChangeItemSprite,
        EventType.ChangeSpriteInSideHand,
    };

    public static bool EventTypeNeedPosition(EventType type)
    {
        return _needPosition.Contains(type);
    }

    public static bool EventTypeNeedID(EventType type)
    {
        return _needID.Contains(type);
    }
}
public enum PositionMode :  byte
{
    MoveLocal = 0,
    SetLocal = 1,
    MoveRelativeToStart = 2,
    MoveRelativeToReloadPoint = 3,
}


