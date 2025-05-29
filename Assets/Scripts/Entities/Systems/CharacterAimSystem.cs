using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]

partial struct CharacterAimSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;
    //private float last;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }
    public void OnUpdate(ref SystemState state)
    {

        deltaTime = SystemAPI.Time.DeltaTime;
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        int i = 0;
        foreach ((RefRW<PlayerInput> input,RefRO<PlayerInputSync> playerInput, RefRW<Hands> hands, RefRW<Character> character, RefRO<LocalToWorld> worldPos, RefRO<GhostOwner> ghostOwner, RefRW<Player> player, Entity entity)
        in SystemAPI.Query<RefRW<PlayerInput>,RefRO<PlayerInputSync>, RefRW<Hands>, RefRW<Character>, RefRO<LocalToWorld>, RefRO<GhostOwner>, RefRW<Player>>().WithNone<NewPlayerTag>().WithAll<Simulate>().WithEntityAccess())
        {
            i++;

           // Debug.Log(networkTime.ServerTick.TickIndexForValidTick + " " +  state.World.Flags);

            if (hands.ValueRO.actionStatus != 0)
            {
                ActionUpdate(hands, player, ref state);
                continue;
            }

            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.main);


            if (playerInput.ValueRO.leftButton.IsSet)
            {
                if (!networkTime.IsFirstTimeFullyPredictingTick) continue;

                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
                quaternion addedRotation = quaternion.Euler(0, 0, math.radians(-110));
                hands.ValueRW.targetRotation = math.normalize(math.mul(addedRotation, transform.Rotation));
                hands.ValueRW.lastPosition = transform.Position;
                hands.ValueRW.lastRotation = transform.Rotation;
                hands.ValueRW.elapsedTime = 0;
                hands.ValueRW.targetPosition = transform.Position + new float3(0.06f, 0, 0);
                hands.ValueRW.actionStatus = 1;
                continue;
            }
            if (playerInput.ValueRO.rightButton.IsSet && !player.ValueRW.isCooldown)
            {
                if (!networkTime.IsFirstTimeFullyPredictingTick) continue;
                //   Debug.Log(aimpoint.Position + " " + state.World.Flags);
                if (state.World.Flags == WorldFlags.GameServer || state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                {
                    Debug.Log(entity + "  ---  " + i);
                    if(state.World.Flags == WorldFlags.GameServer)
                    {
                        Debug.Log(localMain.Rotation + "  " + state.World.Flags);
                        localMain.Rotation = playerInput.ValueRO.handRotation;
                        state.EntityManager.SetComponentData(hands.ValueRO.main, localMain);
                        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<TransformSystemGroup>().Update();
                    }
                    else
                    {
                      //  input.ValueRW.rightButton.Set();
                        Debug.Log("SHOOT!");
                    }

                    Debug.Log(localMain.Rotation + "  " + state.World.Flags);

                    LocalToWorld point = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.aimPoint);
                    LocalToWorld rotation = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.itemInHand);

                    Debug.Log(rotation.Rotation + "  " + state.World.Flags);
                    Debug.Log(point.Rotation + "  " + state.World.Flags);

                    Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                    entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = ghostOwner.ValueRO.NetworkId });
                    entityCommandBuffer.SetComponent(bullet, LocalTransform.FromPosition(point.Position).Rotate(rotation.Rotation));
                    player.ValueRW.isCooldown = true;
                    Debug.Log("...............................................start");

                    if (state.World.Flags == WorldFlags.GameServer)
                    {
                        Debug.Log(" is Cooldown ture " + state.World.Flags);
                        entityCommandBuffer.AddComponent(bullet, new EntityToHide());
                        Bullet bulletComp = SystemAPI.GetComponent<Bullet>(bullet);
                        bulletComp.time = 20;
                        entityCommandBuffer.SetComponent(bullet, bulletComp);
                    }  
                }


                LocalToWorld aimpoint = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.aimPoint);

                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
                LocalToWorld worldPosMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.mainhand);

                quaternion addedRotation = quaternion.Euler(0, 0, math.radians(70));
                hands.ValueRW.targetRotation = math.normalize(math.mul(addedRotation, transform.Rotation));
                hands.ValueRW.lastPosition = transform.Position;
                hands.ValueRW.lastRotation = transform.Rotation;
                hands.ValueRW.elapsedTime = 0;
                hands.ValueRW.targetPosition = transform.Position - new float3(0.06f, 0, 0);
                hands.ValueRW.actionStatus = 2;
                if (state.World.Flags != WorldFlags.GameServer)
                {
                    Sounds.instance.Shot();
                    EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
                    EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)),quaternion.identity);
                    EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation);

                }
                continue;
            }

            LocalTransform localSide = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
            LocalToWorld localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform local = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);


            float3 currentPosition = localToWorld.Position;
            float2 direction = playerInput.ValueRO.sightDirection - new float2(currentPosition.x, currentPosition.y);


            if (!math.any(direction))
                continue;

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


            // localMain.Rotation = mainTargetRotation;
            // localSide.Rotation = sideTargetRotation;
            if (state.World.Flags != WorldFlags.GameServer)
            {
                localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, deltaTime * 20f);
                localSide.Rotation = math.slerp(localSide.Rotation, sideTargetRotation, deltaTime * 5f);
            }
            else
            {
                localMain.Rotation = mainTargetRotation;
                localSide.Rotation = sideTargetRotation;
            }

            //if (direction.y > 0) localMain.Position.z = localMain.Position.y;
            //else localMain.Position.z = 0;


            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSide);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, local);
            UpdateDirectionIndex(new float2(direction.x, direction.y), character, ref state);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }


    //private void GetAnimDir(float3 currentPosition,float2 sightDirection,out quaternion main, out quaternion side)
    //{
    //    float2 direction = sightDirection - new float2(currentPosition.x, currentPosition.y);


    //    if (!math.any(direction)) throw new System.Exception("Direction vector cannot be zero");


    //    direction = math.normalize(direction);

    //    float angle = math.atan2(direction.y, direction.x);
    //    quaternion mainTargetRotation;
    //    quaternion sideTargetRotation;

    //    if (math.abs(angle) > leftSide)
    //    {
    //        if (hands.ValueRO.rotated)
    //        {
    //            localMain = localMain.RotateX(math.radians(180));
    //            hands.ValueRW.rotated = false;
    //            var p = local.Position;
    //            p.z = -0.0001f;
    //            local.Position = p;
    //        }

    //        sideTargetRotation = quaternion.Euler(0, 0, angle - math.radians(90));
    //        angle = -angle;
    //        mainTargetRotation = quaternion.Euler(math.radians(180), 0, angle);
    //    }
    //    else
    //    {
    //        if (!hands.ValueRO.rotated)
    //        {
    //            localMain = localMain.RotateX(math.radians(-180));
    //            hands.ValueRW.rotated = true;
    //            var p = local.Position;
    //            p.z = 0.0001f;
    //            local.Position = p;
    //        }
    //        sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(90));
    //        mainTargetRotation = quaternion.Euler(0, 0, angle);
    //    }
    //}


    private float GetActionTime(int index)
    {
        switch (index)
        {

            case 2: return 0.25f;
            case 1002: return 0.35f;
            default: return 1;
        }
    }
    public void ActionUpdate(RefRW<Hands> hands, RefRW<Player> player, ref SystemState state)
    {
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);

        hands.ValueRW.elapsedTime += deltaTime;

        float t = math.clamp(hands.ValueRO.elapsedTime / GetActionTime(hands.ValueRO.actionStatus), 0f, 1f);
        localTransform.Rotation = math.slerp(localTransform.Rotation, hands.ValueRO.targetRotation, t);
        localTransform.Position = math.lerp(localTransform.Position, hands.ValueRO.targetPosition, t);
      //  Debug.Log(t + " " +  hands.ValueRO.actionStatus + " " + hands.ValueRW.targetPosition);

        if(t == 1)
        {
            if (hands.ValueRO.actionStatus == 1002)
            {
                localTransform.Position = hands.ValueRO.targetPosition;
                localTransform.Rotation = hands.ValueRO.targetRotation;
                hands.ValueRW.actionStatus = 0;
                player.ValueRW.isCooldown = false;

                if (state.World.Flags == WorldFlags.GameServer)
                {
                   player.ValueRW.isCooldown = false;
                    Debug.Log("...............................................KOniec");
                }
            }
            else
            {

                hands.ValueRW.targetRotation = hands.ValueRO.lastRotation;
                hands.ValueRW.targetPosition = hands.ValueRW.lastPosition;

                hands.ValueRW.lastRotation  = localTransform.Rotation;
                hands.ValueRW.lastPosition = localTransform.Position;
                hands.ValueRW.elapsedTime = 0;
                hands.ValueRW.actionStatus = 1002;
            }
        }
        state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.mainhand, localTransform);
    }
    private void UpdateDirectionIndex(float2 dir, RefRW<Character> character, ref SystemState state)
    {
        int newDirIndex = PlayersInputsServiceClientSystem.GetDirectionIndex(dir);
        if (newDirIndex != character.ValueRO.directionHead)
        {
            character.ValueRW.directionHead = newDirIndex;
            SetDirection(character.ValueRO.head, newDirIndex, ref state);
            if (!character.ValueRO.isMove)
            {
                SetDirection(character.ValueRO.body, newDirIndex, ref state);
                character.ValueRW.directionBody = newDirIndex;
            }
        }
    }
    public static void SetDirection(Entity entity, int newIndex, ref SystemState state)
    {
        if (state.EntityManager.HasComponent<SpriteRenderer>(entity))
        {
            SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(entity);
            MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(materialProperty);
            materialProperty.SetInt("_Direction", newIndex);
            spriteRenderer.SetPropertyBlock(materialProperty);
        }
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
