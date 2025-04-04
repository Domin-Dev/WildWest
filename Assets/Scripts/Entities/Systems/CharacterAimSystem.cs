
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

partial struct CharacterAimSystem : ISystem
{


    private static float leftSide = math.PI / 2f;
    private static float max = 0;

    static bool a = false;
    public void OnCreate(ref SystemState state)
    {
        max = 0;
    }
    public void OnUpdate(ref SystemState state)
    {
       
        float deltaTime = SystemAPI.Time.DeltaTime;
        float3 target = (float3)MyTools.GetMouseWorldPosition();

        foreach ((RefRW<Hands> Hands, LocalToWorld worldPos) in SystemAPI.Query<RefRW<Hands>, LocalToWorld>())
        { 
            if(Hands.ValueRO.main == Entity.Null)
            {
                continue;
            }

            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(Hands.ValueRO.main);
            LocalTransform localSide = state.EntityManager.GetComponentData<LocalTransform>(Hands.ValueRO.side);
            LocalToWorld localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(Hands.ValueRO.main);

            float3 currentPosition = localToWorld.Position; 
            float3 direction = target - currentPosition;

            if (!math.any(direction))
                    continue;

            direction.z = 0; 
            direction = math.normalize(direction);

            float angle = math.atan2(direction.y, direction.x); 
            quaternion mainTargetRotation;
            quaternion sideTargetRotation;
            LocalTransform local = state.EntityManager.GetComponentData<LocalTransform>(Hands.ValueRO.itemInHand);

            if (math.abs(angle) > leftSide)
            {
                if (Hands.ValueRO.rotated)
                {
                    localMain = localMain.RotateX(math.radians(180));
                    Hands.ValueRW.rotated = false;
                    var p = local.Position;
                    p.z = -0.0001f;
                    local.Position = p;
                }
               
                sideTargetRotation = quaternion.Euler(0, 0, angle - math.radians(90));
                angle = -angle;
                mainTargetRotation = quaternion.Euler(math.radians(180), 0, angle);
            }
            else
            {
                if (!Hands.ValueRO.rotated)
                {
                    localMain = localMain.RotateX(math.radians(-180));
                    Hands.ValueRW.rotated = true;
                    var p = local.Position;
                    p.z = 0.0001f;
                    local.Position = p;
                }
                sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(90));
                mainTargetRotation = quaternion.Euler(0, 0, angle);
            }
            // state.EntityManager.SetComponentData(Hands.ValueRO.itemInHand, local);


            Debug.Log(mainTargetRotation);
            localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, deltaTime * 10f);
            localSide.Rotation = math.slerp(localSide.Rotation, sideTargetRotation, deltaTime * 2f);


            LocalToWorld toWorld = worldPos;
            LocalToWorld toWorld2 = state.EntityManager.GetComponentData<LocalToWorld>(Hands.ValueRO.itemInHand);
            // float offset =   toWorld2.Position.y - toWorld.Position.y;
            //  Debug.Log(offset + " " + toWorld.Position + " " + toWorld2.Position);
            //if (math.abs(toWorld2.Position.z) > math.abs(max))
            //{
            //    max = toWorld2.Position.z;
            //    Debug.Log(max);
            //}
            //Debug.Log(toWorld2.Position.z);
            //var x = localMain.Position;
            //x.z =  -(worldPos.Position.z - toWorld2.Position.z);
            //localMain.Position = x;

            //  localMain.Position = new float3(localMain.Position.x, localMain.Position.y, offset + 0.0001f);

            //0.265
            state.EntityManager.SetComponentData<LocalTransform>(Hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(Hands.ValueRO.side, localSide);
            state.EntityManager.SetComponentData<LocalTransform>(Hands.ValueRO.itemInHand, local);
        }


            //foreach ((RefRW<LocalTransform> localTransform, 
            //    RefRO<CharacterAim> CharacterAim, 
            //    RefRW<LocalToWorld> localToWorld
            //     ) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<CharacterAim>,RefRW<LocalToWorld>>())
            //{

            //    float3 currentPosition = localToWorld.ValueRO.Position; // Pobranie globalnej pozycji
            //    float3 direction = target - currentPosition;

            //    if (!math.any(direction))
            //        continue;

            //    direction.z = 0; // Ignorujemy oœ Z, obracamy tylko w 2D
            //    direction = math.normalize(direction);

            //    float angle = math.atan2(direction.y, direction.x); // Oblicz k¹t obrotu w 2D
            //    quaternion targetRotation = quaternion.Euler(0, 0, angle);

            //    localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRW.Rotation, targetRotation, deltaTime * 3f);
            //}
        }



    //private float GetAngle(Vector3 mousePos, Transform aimTransform, float addValue)
    //{
    //    Vector3 aimDir = (mousePos - aimTransform.position).normalized;
    //    float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
    //    angle += addValue;
    //    if (angle < 0) angle = 180 + (180 + angle);
    //    if (aimTransform.eulerAngles.z - angle > 180)
    //    {
    //        angle = 360 + angle;
    //    }
    //    else if (aimTransform.eulerAngles.z - angle < -180)
    //    {
    //        angle = -(360 - angle);
    //    }
    //    return angle;
    //}

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }


}


//[BurstCompile]
//public partial struct WorldItemAnimJob : IJobEntity
//{
//    private const float cycleTime = 0.5f;
//    private const float maxDistance = 0.007f;
//    public const float basePos = -0.01f;

//    public float elapsedTime;
//    public void Execute(ref LocalTransform localTransform, WorldItemAnim worldItemAnim)
//    {
//        float pingPongValue = math.sin(elapsedTime / cycleTime * math.PI);
//        float targetY = pingPongValue * maxDistance;
//        localTransform.Position = new float3(localTransform.Position.x, basePos + targetY, localTransform.Position.y);
//    }
//}
