using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.RuleTile.TilingRuleOutput;


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
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

        if(!networkTime.IsFirstTimeFullyPredictingTick) return;

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        var currentTick = networkTime.ServerTick;
        int k = 0;



        deltaTime = (float)SystemAPI.Time.ElapsedTime - (float)last;
        last = SystemAPI.Time.ElapsedTime;

        foreach ((PlayerAspect playerAspect,Entity entity) in SystemAPI.Query<PlayerAspect>().WithNone<NewPlayerTag>().WithAll<Simulate>().WithEntityAccess())
        {
            k++;
            RefRW<Hands> hands = playerAspect.hands;
            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(playerAspect.hands.ValueRO.main);


            var curTargetTicks = new CooldownTargetTick();
            bool isOnCooldown = false;
            NetworkTick latestCooldownTick = NetworkTick.Invalid;
            NetworkTick cooldownEndTick = NetworkTick.Invalid;
            bool buttonIsSet = false;
            bool wasActions = false;
            LastAction? lastAction = state.World.IsServer() ? state.EntityManager.GetComponentData<LastAction>(entity) : null;

            for (var i = 0u; i < networkTime.SimulationStepBatchSize; i++)
            {
                var testTick = currentTick;
                testTick.Subtract(i);

                if (playerAspect.cooldownTargetTick.GetDataAtTick(testTick, out curTargetTicks))
                {
                    wasActions = true;
                    if (playerAspect.input.GetDataAtTick(testTick, out var input))
                    {
                     //   Debug.Log(testTick.TickValue + " " +input.InternalInput.rightButton.Count + "  ---- " + input.InternalInput.dataTick.TickValue);
                    }
                  //  Debug.Log(currentTick.TickValue + " --- " + testTick.TickValue + " " + curTargetTicks.ability.TickValue + " " + curTargetTicks.Tick.TickValue + " " + state.World.Flags + " " + networkTime.SimulationStepBatchSize + " " + buttonIsSet);
                    if (currentTick.IsNewerThan(curTargetTicks.ability))
                    {
                        latestCooldownTick = testTick;
                        cooldownEndTick = curTargetTicks.ability;
                        break;
                    }
                }
            }
          

            if(wasActions && cooldownEndTick == NetworkTick.Invalid)
            {
                isOnCooldown = true;
            }
            else
            {
                if (lastAction.HasValue && lastAction.Value.tick != NetworkTick.Invalid)
                {
                    isOnCooldown = lastAction.Value.tick.IsNewerThan(cooldownEndTick);
                }
                else
                    isOnCooldown = false;
            }



            if (!isOnCooldown)
            {
                NetworkTick tick;
                if (latestCooldownTick == NetworkTick.Invalid)
                    tick = currentTick;
                else
                    tick = latestCooldownTick;


                if (playerAspect.input.GetDataAtTick(tick, out var input1))
                {
                    tick.Subtract(1);
                    if (playerAspect.input.GetDataAtTick(tick, out var input2))
                    {
                        uint counter2 = 0;
                        if (input2.InternalInput.dataTick != tick && input2.InternalInput.dataTick != NetworkTick.Invalid)
                        {
                            tick = input2.InternalInput.dataTick;
                            tick.Subtract(1);
                            if (playerAspect.input.GetDataAtTick(tick, out var input3))
                            {
                                counter2 = input3.InternalInput.rightButton.Count;
                            }
                        }
                        else
                            counter2 = input2.InternalInput.rightButton.Count;
                        buttonIsSet = counter2 - input1.InternalInput.rightButton.Count != 0;
                    }
                }


                if (buttonIsSet)
                {
                    if (state.World.Flags == WorldFlags.GameServer || state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                    {
                        if (state.World.Flags == WorldFlags.GameServer)
                        {
                            lastAction = new LastAction() { tick = currentTick};
                            localMain.Rotation = playerAspect.playerInputSync.ValueRO.handRotation;
                            state.EntityManager.SetComponentData(playerAspect.hands.ValueRO.main, localMain);
                            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<TransformSystemGroup>().Update();
                        }

                        LocalToWorld point = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.aimPoint);
                        LocalToWorld rotation = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.itemInHand);


                      
                        Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                        entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = playerAspect.networkId });
                      //  Debug.Log("<Color=#00ff00>  Position! " + point.Position + " " + rotation.Rotation);
                        LocalTransform lt = LocalTransform.FromPosition(point.Position).Rotate(rotation.Rotation);
                        entityCommandBuffer.SetComponent(bullet, lt);


                        //  float3 v3 = lt.Right();
                        //entityCommandBuffer.AddComponent(entity, new ForceImpulse2D() { Value = new float2(-v3.x, -v3.y) });



                        if (state.World.Flags == WorldFlags.GameServer)
                        {
                            entityCommandBuffer.AddComponent(bullet, new GhostChunk().StartValues());
                            entityCommandBuffer.AddComponent(bullet, new NewChunk());
    
                            entityCommandBuffer.AddComponent(bullet, new EntityToHide());
                            NewBullet bulletComp = SystemAPI.GetComponent<NewBullet>(bullet);
                            bulletComp.isOnServer = true;
                            entityCommandBuffer.SetComponent(bullet, bulletComp);
                        }
                        else
                        {
                            var newCooldownTargetTick = currentTick;

                            //23u
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


                    if (hands.ValueRW.actionStatus != 0)
                    {
                        transform.Position = hands.ValueRO.targetPosition;
                        transform.Rotation = hands.ValueRO.targetRotation;
                    }

                    SetActionStatus(ref state, 2, hands, transform.Rotation, math.normalize(math.mul(addedRotation, transform.Rotation)), transform.Position, transform.Position - new float3(0.06f, 0, 0));

                    if (state.World.Flags != WorldFlags.GameServer)
                    {
                        Sounds.instance.Shot();
                        EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
                        EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)), quaternion.identity);
                        EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation);
                    }

                    if (lastAction.HasValue) entityCommandBuffer.SetComponent(entity, lastAction.Value);
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



            playerAspect.input.GetDataAtTick(currentTick, out var dir);

            float2 direction = dir.InternalInput.sightDirection - new float2(currentPosition.x, currentPosition.y);

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


    private void SetActionStatus(ref SystemState state,int index, RefRW<Hands> hands, quaternion lastRot, quaternion targetRot,float3 lastPos, float3 targetPos)
    {
        hands.ValueRW.lastRotation = lastRot;
        hands.ValueRW.lastPosition = lastPos;

        hands.ValueRW.elapsedTime = 0;

        hands.ValueRW.targetPosition = targetPos;
        hands.ValueRW.targetRotation = targetRot;

        hands.ValueRW.actionStatus = index;
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

        if (direction.y > 0) localMain.Position.z = 0.0011f;
        else localMain.Position.z = -0.001f;
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
