using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(VariableSynchronizationServerSystem))]
[UpdateAfter(typeof(CollisionSystem))]

 partial struct CharacterAimSystem : ISystem
{

    private float deltaTime;

    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;

    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> containersLookup;


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();  

        playerNeedChunkLookup = state.GetBufferLookup<PlayersNeedChunk>(true);
        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>(true);
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>(true);

    }


    // public void OnUpdate(ref SystemState state)
    // {

    //     //  var ecbSingleton = // SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
    //     // EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
    //     NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

    

    //     EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
    //     EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    //     var currentTick = networkTime.ServerTick;
    //     int k = 0;



    //     deltaTime = (float)SystemAPI.Time.ElapsedTime - (float)last;
    //     last = SystemAPI.Time.ElapsedTime;

    //     foreach ((PlayerAspect playerAspect,Entity entity) in SystemAPI.Query<PlayerAspect>().WithNone<NewPlayerTag>().WithAll<Simulate>().WithEntityAccess())
    //     {
    //         k++;
    //         RefRW<Hands> hands = playerAspect.hands;
    //         LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(playerAspect.hands.ValueRO.main);


    //         var curTargetTicks = new CooldownTargetTick();
    //         bool isOnCooldown = false;
    //         NetworkTick latestCooldownTick = NetworkTick.Invalid;
    //         NetworkTick cooldownEndTick = NetworkTick.Invalid;
    //         bool buttonIsSet = false;
    //         bool wasActions = false;
    //         LastAction? lastAction = state.World.IsServer() ? state.EntityManager.GetComponentData<LastAction>(entity) : null;

    //         for (var i = 0u; i < networkTime.SimulationStepBatchSize; i++)
    //         {
    //             var testTick = currentTick;
    //             testTick.Subtract(i);

    //             if (playerAspect.cooldownTargetTick.GetDataAtTick(testTick, out curTargetTicks))
    //             {
    //                 wasActions = true;
    //                 if (playerAspect.input.GetDataAtTick(testTick, out var input))
    //                 {
    //                     Debug.Log(testTick.TickValue + " " +input.InternalInput.rightButton.Count + "  ---- " + input.InternalInput.dataTick.TickValue);
    //                 }
    //               //  Debug.Log(currentTick.TickValue + " --- " + testTick.TickValue + " " + curTargetTicks.ability.TickValue + " " + curTargetTicks.Tick.TickValue + " " + state.World.Flags + " " + networkTime.SimulationStepBatchSize + " " + buttonIsSet);
    //                 if (currentTick.IsNewerThan(curTargetTicks.ability))
    //                 {
    //                     latestCooldownTick = testTick;
    //                     cooldownEndTick = curTargetTicks.ability;
    //                     break;
    //                 }
    //             }
    //         }
          

    //         if(wasActions && cooldownEndTick == NetworkTick.Invalid)
    //         {
    //             isOnCooldown = true;
    //         }
    //         else
    //         {
    //             if (lastAction.HasValue && lastAction.Value.tick != NetworkTick.Invalid)
    //             {
    //                 //Debug.Log(cooldownEndTick + " " + lastAction.Value.tick);
    //                 isOnCooldown = lastAction.Value.tick.IsNewerThan(cooldownEndTick);
    //             }
    //             else
    //                 isOnCooldown = false;
    //         }


    //         NetworkTick tick = currentTick;
    //         if (!isOnCooldown &&  networkTime.IsFirstTimeFullyPredictingTick)
    //         {
    //             if (latestCooldownTick == NetworkTick.Invalid)
    //                 tick = currentTick;
    //             else
    //                 tick = latestCooldownTick;


    //             Debug.Log("tick!!");

    //             if (playerAspect.input.GetDataAtTick(tick, out var input1))
    //             {
    //                 tick.Subtract(1);
    //                 if (playerAspect.input.GetDataAtTick(tick, out var input2))
    //                 {
    //                     uint counter2 = 0;
    //                     if (input2.InternalInput.dataTick != tick && input2.InternalInput.dataTick != NetworkTick.Invalid)
    //                     {
    //                         tick = input2.InternalInput.dataTick;
    //                         tick.Subtract(1);
    //                         if (playerAspect.input.GetDataAtTick(tick, out var input3))
    //                         {
    //                             counter2 = input3.InternalInput.rightButton.Count;
    //                         }
    //                     }
    //                     else
    //                         counter2 = input2.InternalInput.rightButton.Count;
    //                     buttonIsSet = counter2 - input1.InternalInput.rightButton.Count != 0;
    //                 }
    //             }


    //             if (buttonIsSet)
    //             {
    //                 if (state.World.Flags == WorldFlags.GameServer || state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
    //                 {
    //                     if (state.World.Flags == WorldFlags.GameServer)
    //                     {
    //                         lastAction = new LastAction() { tick = currentTick};
    //                         localMain.Rotation = playerAspect.playerInputSync.ValueRO.handRotation;
    //                         state.EntityManager.SetComponentData(playerAspect.hands.ValueRO.main, localMain);
    //                         World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<TransformSystemGroup>().Update();
    //                     }

    //                     LocalToWorld point = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.aimPoint);
    //                     LocalToWorld rotation = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.itemInHand);


                      
    //                     Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
    //                     entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = playerAspect.networkId });
    //                     Debug.Log("<Color=#00ff00>  Position! " + point.Position + " " + rotation.Rotation);
    //                     LocalTransform lt = LocalTransform.FromPosition(point.Position).Rotate(rotation.Rotation);
    //                     entityCommandBuffer.SetComponent(bullet, lt);


    //                     //  float3 v3 = lt.Right();
    //                     //entityCommandBuffer.AddComponent(entity, new ForceImpulse2D() { Value = new float2(-v3.x, -v3.y) });



    //                     if (state.World.Flags == WorldFlags.GameServer)
    //                     {
    //                         entityCommandBuffer.AddComponent(bullet, new GhostChunk().StartValues());
    //                         entityCommandBuffer.AddComponent(bullet, new NewChunk());
    
    //                         entityCommandBuffer.AddComponent(bullet, new EntityToHide());
    //                         NewBullet bulletComp = SystemAPI.GetComponent<NewBullet>(bullet);
    //                         bulletComp.isOnServer = true;
    //                         entityCommandBuffer.SetComponent(bullet, bulletComp);
    //                     }
                        


    //                         var newCooldownTargetTick = currentTick;

    //                         //23u
    //                         //28u
    //                         newCooldownTargetTick.Add(38u);
    //                         curTargetTicks.ability = newCooldownTargetTick;



    //                         var nextTick = currentTick;
    //                         nextTick.Add(1u);
    //                         curTargetTicks.Tick = nextTick;

    //                         playerAspect.cooldownTargetTick.AddCommandData(curTargetTicks);
                        
    //                 }


    //                 LocalToWorld aimpoint = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.aimPoint);
    //                 LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerAspect.hands.ValueRO.mainhand);
    //                 LocalToWorld worldPosMainHand = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.mainhand);
    //                 quaternion addedRotation = quaternion.Euler(0, 0, math.radians(70));


    //                 if (hands.ValueRW.actionStatus != 0)
    //                 {
    //                     transform.Position = hands.ValueRO.targetPosition;
    //                     transform.Rotation = hands.ValueRO.targetRotation;
    //                 }

    //                 SetActionStatus(ref state, 2, hands, transform.Rotation, math.normalize(math.mul(addedRotation, transform.Rotation)), transform.Position, transform.Position - new float3(0.06f, 0, 0));

    //                 if (state.World.Flags != WorldFlags.GameServer)
    //                 {
    //                     Sounds.instance.Shot();
    //                     EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
    //                     EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)), quaternion.identity);
    //                     EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation);
    //                 }

    //                 if (lastAction.HasValue) entityCommandBuffer.SetComponent(entity, lastAction.Value);
    //                 continue;
    //             }
    //         }

    //         if (hands.ValueRO.actionStatus != 0)
    //         {
    //             ActionUpdate(hands, playerAspect.player, ref state);
    //         }

    //         LocalTransform localSideHand = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
    //         LocalToWorld worldMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
    //         LocalTransform localItem = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);

    //         float3 currentPosition = worldMainHand.Position;


    //         playerAspect.input.GetDataAtTick(tick, out var dir);

    //         float2 direction = dir.InternalInput.sightDirection - new float2(currentPosition.x, currentPosition.y);

    //         if (!math.any(direction))
    //             continue;



    //         UpdateAimSystem(direction, ref localSideHand, ref localItem, ref localMain, hands);

    //         state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
    //         state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSideHand);
    //         state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, localItem);
    //         UpdateDirectionIndex(new float2(direction.x, direction.y),playerAspect.character, ref state);


    //     }

    //     entityCommandBuffer.Playback(state.EntityManager);
    //     entityCommandBuffer.Dispose();
    // }
    public void OnUpdate(ref SystemState state)
    {
        playerNeedChunkLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);

        //var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        //EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);


        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        SystemAPI.TryGetSingletonBuffer<LoadedChunks>(out var loadedChunks,true);



        deltaTime = SystemAPI.Time.DeltaTime;
        var currentTick = networkTime.ServerTick;
        if(!networkTime.IsFirstTimeFullyPredictingTick) return;
    
        playerNeedChunkLookup.Update(ref state);

        foreach ((PlayerAspect playerAspect,Entity entity) in SystemAPI.Query<PlayerAspect>().WithNone<NewPlayerTag>().WithAll<Simulate>().WithEntityAccess())
        {
            if (state.World.IsClient() && !state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                continue;

            LocalToWorld worldMainHand = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.main);
            float rot = playerAspect.aimRotation.ValueRO.angle;   

            for (var i = 1u; i <= networkTime.SimulationStepBatchSize; i++)
            {
                var testTick = currentTick;
                testTick.Subtract((uint)networkTime.SimulationStepBatchSize - i);           
                if(testTick.IsValid && playerAspect.input.GetDataAtTick(testTick, out var input))
                {
                    float3 currentPosition = worldMainHand.Position;
                    float2 direction = input.InternalInput.sightDirection - new float2(currentPosition.x, currentPosition.y);
                    if (!math.any(direction) || input.InternalInput.SightDirectionIsEmpty())
                        continue;

                    CalculateNextRotation(ref rot, direction,0.5f);
                    if(!playerAspect.cooldown.ValueRO.cooldownTick.IsValid || testTick.IsNewerThan(playerAspect.cooldown.ValueRO.cooldownTick))
                    {
                        testTick.Subtract(1);
                        if (playerAspect.input.GetDataAtTick(testTick, out var input2))
                        {
                            uint counter2 = input2.InternalInput.rightButton.Count;
                            if(counter2 - input.InternalInput.rightButton.Count != 0)
                            {  
                                if(!EQHelper.TryGetPlayerContainer(containersLookup,entity,EquipmentConfig.itemInHand_ContainerIndex,out var playerContainer))
                                    break;

                                if(!EQHelper.TryGetBufferIndex(slotsLookup,0,playerContainer.Value.entity,out int itemId, out int bufferIndex))
                                    break;

                                if(!ItemsAsset.instance.TryGetItem<RangedWeapon>(itemId,out var weapon))
                                    break;




                                testTick.Add(1u);
                                var cooldownTick = testTick;
                                cooldownTick.Add(weapon.cooldown);
                                playerAspect.cooldown.ValueRW.cooldownTick = cooldownTick;


                               
                                
                                        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<TransformSystemGroup>().Update();

                                        LocalTransform localSideHand = state.EntityManager.GetComponentData<LocalTransform>( playerAspect.hands.ValueRO.side);
                                        LocalTransform localItem = state.EntityManager.GetComponentData<LocalTransform>( playerAspect.hands.ValueRO.itemInHand);
                                        LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>( playerAspect.hands.ValueRO.main);
                                          
                                      
                                        CharacterHandsSystem.GetHandsRotation(rot,ref localSideHand, ref localItem, ref localMain,playerAspect.hands);


                                        state.EntityManager.SetComponentData(playerAspect.hands.ValueRO.main, localMain);

                
                                        LocalToWorld point = state.EntityManager.GetComponentData<LocalToWorld>(playerAspect.hands.ValueRO.aimPoint);



                                        Debug.Log("point " + point.Position);

                                        uint seed = (uint)testTick.TickIndexForValidTick * 747796405u + 2891336453u;
                                        Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);
                                        float spread = 1f * Mathf.Deg2Rad;

                                        for(int k = 0; k < 1; k++)
                                        {
                                            Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                                            entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = playerAspect.networkId });
                                            Debug.Log(state.World.Unmanaged.IsServer()+ " - " + testTick.TickIndexForValidTick + "<Color=#00ff00>  Position! " + point.Position + " " + quaternion.Euler(0, 0, rot));
                                        
                                            
                                            LocalTransform lt = LocalTransform.FromPosition(point.Position).Rotate(quaternion.Euler(0, 0, rot + random.NextFloat(-1 * spread,spread)));
                                            entityCommandBuffer.SetComponent(bullet, lt);


                                            if (state.World.Flags == WorldFlags.GameServer)
                                            {
                                                entityCommandBuffer.AddComponent(bullet, new GhostChunk().StartValues());
                                                entityCommandBuffer.AddComponent(bullet, new NewChunk());
                                                entityCommandBuffer.AddComponent(bullet, new EntityToHide());
                                                NewBullet bulletComp = SystemAPI.GetComponent<NewBullet>(bullet);
                                                bulletComp.isOnServer = true;
                                                entityCommandBuffer.SetComponent(bullet, bulletComp);
                                                RPCHelper.SendEventsToClients<PlayerActionRPC>(ref state,playerNeedChunkLookup,loadedChunks,entityCommandBuffer,playerAspect.networkId,playerAspect.ghostChunk.ValueRO.GetChunk(),testTick);
                                            }
                                        }
                                        
                                        

                                        if(state.World.IsClient())
                                        {
                                            EntityHelper.CreateEntityWithComponent(entityCommandBuffer,new PlayerActionRPC()
                                            {
                                                networkID = playerAspect.networkId
                                            });
                                        }


                                            
                                
                            }
                        }
                    }
          
                }
            }
            playerAspect.aimRotation.ValueRW.angle = rot;
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
 



    private void CalculateNextRotation(ref float currentAngle, Vector2 direction, float maxStep = 0.02f)
    {
        direction = math.normalize(direction);
        float angle = math.atan2(direction.y, direction.x);
        float delta = math.atan2(
            math.sin(angle - currentAngle),
            math.cos(angle - currentAngle)
        );

        delta = math.clamp(delta, -maxStep, maxStep);
        currentAngle += delta;
            currentAngle = math.atan2(
        math.sin(currentAngle),
        math.cos(currentAngle)
        );
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
