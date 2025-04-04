
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

partial struct CharacterAimSystem : ISystem
{


    private static float leftSide = math.PI / 2f;

    private float deltaTime;
    public void OnCreate(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
       
        deltaTime = SystemAPI.Time.DeltaTime;
        float3 target = (float3)MyTools.GetMouseWorldPosition();

        bool hit = Input.GetMouseButtonDown(0);


        foreach ((RefRW<Hands> hands, LocalToWorld worldPos) in SystemAPI.Query<RefRW<Hands>, LocalToWorld>())
        { 
            if(hands.ValueRO.main == Entity.Null) continue;
            if (hands.ValueRO.actionStatus != 0)
            {
                ActionUpdate(hands, ref state);
                continue;
            }
            if (hit)
            {

                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.hand);

                quaternion addedRotation = quaternion.Euler(0, 0, math.radians(-110));
                hands.ValueRW.targetRotation = math.normalize(math.mul(addedRotation, transform.Rotation));
                hands.ValueRW.lastPosition = transform.Position;
                hands.ValueRW.targetPosition = transform.Position + new float3(0.04f, 0,0);
                hands.ValueRW.actionStatus = 1;
            }


            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.main);
            LocalTransform localSide = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
            LocalToWorld localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform local = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);

            float3 currentPosition = localToWorld.Position; 
            float3 direction = target - currentPosition;

            if (!math.any(direction))
                    continue;

            direction.z = 0; 
            direction = math.normalize(direction);

            float angle = math.atan2(direction.y, direction.x); 
            quaternion mainTargetRotation;
            quaternion sideTargetRotation;

            if (math.abs(angle) > leftSide)
            {
                if (hands.ValueRO.rotated)
                {
                    localMain = localMain.RotateX(math.radians(180));
                    hands.ValueRW.rotated = false;
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
                if (!hands.ValueRO.rotated)
                {
                    localMain = localMain.RotateX(math.radians(-180));
                    hands.ValueRW.rotated = true;
                    var p = local.Position;
                    p.z = 0.0001f;
                    local.Position = p;
                }
                sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(90));
                mainTargetRotation = quaternion.Euler(0, 0, angle);
            }

            localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, deltaTime * 10f);
            localSide.Rotation = math.slerp(localSide.Rotation, sideTargetRotation, deltaTime * 2f);

            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSide);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, local);
        }
    }

    public void ActionUpdate(RefRW<Hands> hands, ref SystemState state)
    {
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.hand);

        localTransform.Rotation = math.slerp(localTransform.Rotation, hands.ValueRO.targetRotation, deltaTime * 15f);
        localTransform.Position = math.lerp(localTransform.Position, hands.ValueRO.targetPosition, deltaTime * 20);


        if (MyTools.EqualFloat3(localTransform.Position,hands.ValueRO.targetPosition,0.005f) && MyTools.EqualQuaternions(localTransform.Rotation,hands.ValueRO.targetRotation,0.985f))
        {
            if (MyTools.EqualQuaternions(quaternion.identity, localTransform.Rotation))
            {
                localTransform.Position = hands.ValueRW.lastPosition;
                localTransform.Rotation = quaternion.identity;
                hands.ValueRW.actionStatus = 0;
            }


            hands.ValueRW.targetRotation = quaternion.identity;
            hands.ValueRW.targetPosition = hands.ValueRW.lastPosition;
        }
        state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.hand, localTransform);
    }

 

    public void SetAttackVector(Vector3 Angle, Vector3 position)
    {
        //canAttack = false;
        //Timer.Create(setTime, () => { canAttack = true; return false; });
        //attackItem = fliper;

        //attackAngle = hand.localEulerAngles - Angle;
        //this.firstHand.AttackSwitch(true);
        //lastWeaponPosition = attackItem.localPosition;
        //attackVector = position + attackItem.localPosition;
    }



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
