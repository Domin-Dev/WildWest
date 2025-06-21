using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(VariableSynchronizationServerSystem))]
[UpdateAfter(typeof(CollisionSystem))]

partial struct CharacterAimSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;
    private double last;
    public void OnCreate(ref SystemState state)
    {
        last = 0;
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
       // state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        //  var ecbSingleton = // SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;


        int k = 0;

        deltaTime = (float)SystemAPI.Time.ElapsedTime - (float)last;
        last = SystemAPI.Time.ElapsedTime;

        foreach ((PlayerAspect playerAspect,Entity entity) in SystemAPI.Query<PlayerAspect>().WithNone<NewPlayerTag>().WithAll<Simulate>().WithEntityAccess())
        {
            if(state.World.IsServer() || state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
            {
                playerAspect.playerInputSync.ValueRW.sightDirection = playerAspect.playerInput.ValueRO.sightDirection;
                playerAspect.playerInputSync.ValueRW.leftButton = playerAspect.playerInput.ValueRO.leftButton;
                playerAspect.playerInputSync.ValueRW.rightButton = playerAspect.playerInput.ValueRO.rightButton;
                playerAspect.playerInputSync.ValueRW.handRotation = playerAspect.playerInput.ValueRO.handRotation;
            }

            k++;
            RefRW<Hands> hands = playerAspect.hands;
            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(playerAspect.hands.ValueRO.main);

            var isOnCooldown = true;
            var curTargetTicks = new CooldownTargetTick();

            for (var i = 0u; i < networkTime.SimulationStepBatchSize; i++)
            {
                var testTick = currentTick;
                testTick.Subtract(i);

                if (!playerAspect.cooldownTargetTick.GetDataAtTick(testTick, out curTargetTicks))
                {
                    curTargetTicks.ability = NetworkTick.Invalid;
                }

                if (curTargetTicks.ability == NetworkTick.Invalid ||
                    !curTargetTicks.ability.IsNewerThan(currentTick))
                {
                    isOnCooldown = false;
                    break;
                }
            }

            if (!isOnCooldown)
            {
                if (playerAspect.playerInputSync.ValueRO.rightButton.IsSet)
                {

                    if (state.World.Flags == WorldFlags.GameServer || state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                    {
                        if (state.World.Flags == WorldFlags.GameServer)
                        {
                            Debug.Log(localMain.Rotation + "  " + state.World.Flags);
                            localMain.Rotation = playerAspect.playerInputSync.ValueRO.handRotation;
                            state.EntityManager.SetComponentData(playerAspect.hands.ValueRO.main, localMain);
                            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<TransformSystemGroup>().Update();
                        }

                        LocalToWorld point = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.aimPoint);
                        LocalToWorld rotation = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.itemInHand);


                        Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                         Debug.Log(SystemAPI.GetComponentRO<LocalTransform>(bullet).ValueRO.Position);

                        entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = playerAspect.networkId });
                        Debug.Log(LocalTransform.FromPosition(point.Position).Rotate(rotation.Rotation));
                        entityCommandBuffer.SetComponent(bullet, LocalTransform.FromPosition(point.Position).Rotate(rotation.Rotation));

                    //    Debug.Log(SystemAPI.getcomponentro<LocalTransform>(bullet).ValueRO.Position);

                        if (state.World.Flags == WorldFlags.GameServer)
                        {
                            entityCommandBuffer.AddComponent(bullet, new EntityToHide());
                            NewBullet bulletComp = SystemAPI.GetComponent<NewBullet>(bullet);
                            bulletComp.isOnServer = true;
                            entityCommandBuffer.SetComponent(bullet, bulletComp);
                        }
                        else
                        {
                            var newCooldownTargetTick = currentTick;
                            newCooldownTargetTick.Add(28u);
                            curTargetTicks.ability = newCooldownTargetTick;

                            var nextTick = currentTick;
                            nextTick.Add(1u);
                            curTargetTicks.Tick = nextTick;

                            playerAspect.cooldownTargetTick.AddCommandData(curTargetTicks);
                        }
                    }


                    LocalToWorld aimpoint = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.aimPoint);
                    LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerAspect.hands.ValueRO.mainhand);
                    LocalToWorld worldPosMainHand = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.mainhand);
                    quaternion addedRotation = quaternion.Euler(0, 0, math.radians(70));


                    playerAspect.hands.ValueRW.targetRotation = math.normalize(math.mul(addedRotation, transform.Rotation));
                    playerAspect.hands.ValueRW.lastPosition = transform.Position;

                    hands.ValueRW.lastRotation = transform.Rotation;
                    hands.ValueRW.elapsedTime = 0;
                    hands.ValueRW.targetPosition = transform.Position - new float3(0.06f, 0, 0);
                    hands.ValueRW.actionStatus = 2;
                    if (state.World.Flags != WorldFlags.GameServer)
                    {
                        Sounds.instance.Shot();
                        EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
                        EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)), quaternion.identity);
                        EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation);
                    }
                    continue;
                }
            }


            if (hands.ValueRO.actionStatus != 0)
            {
                ActionUpdate(hands, playerAspect.player, ref state);
            }

            LocalTransform localSideHand = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
            LocalToWorld worldMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform localItem = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);

            float3 currentPosition = worldMainHand.Position;
            float2 direction = playerAspect.playerInputSync.ValueRO.sightDirection - new float2(currentPosition.x, currentPosition.y);

            if (!math.any(direction))
                continue;

            UpdateAimSystem(direction, ref localSideHand, ref localItem, ref localMain, hands);

            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSideHand);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, localItem);
            UpdateDirectionIndex(new float2(direction.x, direction.y),playerAspect.character, ref state);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void UpdateAimSystem(float2 direction,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands)
    {
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
                var p = localItem.Position;
                p.z = -0.0001f;
                localItem.Position = p;
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
                var p = localItem.Position;
                p.z = 0.0001f;
                localItem.Position = p;
            }
            sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(90));
            mainTargetRotation = quaternion.Euler(0, 0, angle);
        }

        localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, deltaTime * 15);
        localSideHand.Rotation = math.slerp(localSideHand.Rotation, sideTargetRotation, deltaTime * 5f);

        if (direction.y > 0) localMain.Position.z = localMain.Position.y;
        else localMain.Position.z = 0;      
    }

    private float GetActionTime(int index)
    {
        switch (index)
        {
            case 2: return 0.2f;
            case 1002: return 0.2f;
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
