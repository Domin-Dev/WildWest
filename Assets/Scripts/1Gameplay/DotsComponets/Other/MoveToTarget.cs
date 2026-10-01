using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct MoveToTarget : IComponentData
{
    public NetworkTick startTick;

    public float2 target;
    public Entity targetEntity;
    public bool destroy;


    public float2 startPosition;
    public float duration; 
}


