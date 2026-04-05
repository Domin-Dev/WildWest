using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;




public struct Hands : IComponentData
{
    public Entity main;
    public Entity side;
    public Entity itemInMainHand;

    public Entity itemInSideHand;




    public Entity mainhand;
    public Entity sidehand;





    public Entity aimPoint;
    public Entity reloadPoint;
    public Entity hitboxPoint;


    public bool rotated;

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

