using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.XR;

public struct Hands : IComponentData
{
    public Entity main;
    public Entity side;
    public Entity itemInHand;
    public Entity mainhand;
    public Entity sidehand;

    public int actionStatus;

    public quaternion targetRotation;
    public float3 targetPosition;

    public float3 lastPosition;

    public bool rotated;
    public int itemID;



}

