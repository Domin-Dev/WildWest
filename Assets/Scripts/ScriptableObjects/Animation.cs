


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


    public AnimationFrames GetAnimationFrame()
    {
        return new AnimationFrames()
        {
            duration = Duration,
            targetRotation = Rotation,
            targetPosition = Move,
            positionMode = positionMode,
            processed = false
        };
    }
}


[System.Serializable]
public class AnimationEvent
{
    public EventType EventType;
    public int id;
}


public enum BodyPartType :  byte
{
    MainHand,
    SideHand
}

public enum EventType :  byte
{
    Sound,
    SpawnParticle
}

public enum PositionMode :  byte
{
    Local = 0,
    RelativeToStart = 1,
}

