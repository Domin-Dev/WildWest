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



    public Entity aimPoint;
    public Entity reloadPoint;
    public Entity hitboxPoint;


    public int actionStatus;

    public quaternion targetRotation;
    public float3 targetPosition;

    public float3 lastPosition;
    public quaternion lastRotation;

    public bool rotated;
    public float elapsedTime;


    public Entity GetBodyPart(BodyPartType bodyPartType)
    {
        switch(bodyPartType)
        {
            case BodyPartType.MainHand :
                return mainhand;
            case BodyPartType.SideHand :
                return sidehand;
        }
        return Entity.Null;
    }
}

