using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor;
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
    private BufferLookup<EntityContainers> containersLookup;
    private BufferLookup<LinkedContainers> linkedContainersLookup;

    private float simulationTickDelta;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<ShootingConfig>();

        playerNeedChunkLookup = state.GetBufferLookup<PlayersNeedChunk>(true);
        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        containersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        linkedContainersLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
        
        simulationTickDelta = 1f / NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }


  
    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;
        if(!networkTime.IsFirstTimeFullyPredictingTick) return;

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        ShootingConfig shootingConfig = SystemAPI.GetSingleton<ShootingConfig>();
        SystemAPI.TryGetSingletonBuffer<LoadedChunks>(out var loadedChunks,true);
    
        playerNeedChunkLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);
        linkedContainersLookup.Update(ref state);
        
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
                    float rawSpread = DeltaAngle(playerAspect.aimRotation.ValueRO.angle,rot);
                    float newSpread = playerAspect.spread.ValueRO.Spread + rawSpread * shootingConfig.sensitivityPlayerAim;
                    if(newSpread > 0)
                    {
                        newSpread -= shootingConfig.spreadRecovery;
                    }

                  //  Debug.Log(testTick.TickIndexForValidTick + "  " + state.World.Flags  + playerAspect.spread.ValueRO.Spread + " new -> " + newSpread + " , " + rawSpread + "(" + playerAspect.aimRotation.ValueRO.angle + " "  + rot + ")");

                    playerAspect.spread.ValueRW.Spread = math.clamp(newSpread,0,shootingConfig.maxSpread);
                    
                   // Debug.Log(" jest input!! "+ state.World.Flags + " " + testTick.TickIndexForValidTick + " " + playerAspect.cooldown.ValueRO.cooldownTick.TickIndexForValidTick + " " +(playerAspect.cooldown.ValueRO.startCooldown.IsValid ? playerAspect.cooldown.ValueRO.startCooldown.TickIndexForValidTick : "null"));
                    
                    bool isCooldown = !playerAspect.cooldown.ValueRO.cooldownTick.IsValid || testTick.IsNewerThan(playerAspect.cooldown.ValueRO.cooldownTick) ||
                    (playerAspect.cooldown.ValueRO.startCooldown.IsValid && playerAspect.cooldown.ValueRO.startCooldown.IsNewerThan(testTick));
                    playerAspect.aimRotation.ValueRW.angle = rot;
                    
                    if(isCooldown)
                    {
                        testTick.Subtract(1);
                        if (playerAspect.input.GetDataAtTick(testTick, out var input2))
                        {
                            uint counter2 = input2.InternalInput.rightButton.Count;
                            if(counter2 - input.InternalInput.rightButton.Count != 0)
                            {  
                                Debug.Log("shoot!!! " + rot + "  spread => " +  playerAspect.spread.ValueRO.Spread   +  " "   + state.World.Flags + " " + testTick.TickIndexForValidTick +  " cool = " +  playerAspect.cooldown.ValueRO.cooldownTick.TickIndexForValidTick);

                                if(!EQHelper.TryGetPlayerContainer(containersLookup,entity,EquipmentConfig.hotBar_ContainerIndex,out var playerContainer))
                                    break;

                                if(!EQHelper.TryGetBufferIndex(slotsLookup,playerAspect.playerInputSync.ValueRO.slotInHand,playerContainer.Value.entity,out int itemId, out int bufferIndex))
                                    break;

                                if(!ItemsAsset.instance.TryGetItem<RangedWeapon>(itemId,out var weapon))
                                    break;


                                int ammoID = -1;
                                float reloadCooldown = 0;

                                if(weapon.hasMagazine)
                                {
                                    var magazine = EQHelper.ReadLinkedContainer(slotsLookup,linkedContainersLookup,playerContainer.Value.entity,playerAspect.playerInputSync.ValueRO.slotInHand, out Entity linkedContainerEntity);
                                    if(magazine == null  || magazine.Length == 0)
                                    {
                                        var emptyMagazine = new EmptyMagazineRPC()
                                        {
                                            networkID = playerAspect.networkId,
                                            itemID = itemId,
                                            tick = testTick
                                        };   
                                        if(state.World.IsServer())
                                            RPCHelper.SendEventsToClients<EmptyMagazineRPC>(emptyMagazine,ref state,playerNeedChunkLookup,loadedChunks,entityCommandBuffer,playerAspect.networkId,entity,playerAspect.ghostChunk.ValueRO.GetChunk(),testTick);
                                        else                                          
                                            EntityHelper.CreateEntityWithComponent(entityCommandBuffer,emptyMagazine);  


                                        playerAspect.cooldown.ValueRW.cooldownTick = EntityHelper.AddTime(testTick,0.5f);
                                        break;
                                    }

                                    ammoID = magazine[0].itemId;
                                    if(state.World.IsServer())
                                    {
                                        EQHelper.SubtractItem(slotsLookup,linkedContainerEntity,0,out var removedValue,1,false);  
                                        EQHelper.SendEvents(entityCommandBuffer, playerAspect.networkId,new EquipmentEvent(EquipementEventFlags.UpdateWeaponMagazine));                                  
                                    }
                                }
                                else
                                {
                                    var slots = EQHelper.TryFindItemWithTag(ref state,slotsLookup,containersLookup,entity,weapon.ammoTagID,out var aggregated,out int counter);
                                    if(counter == 0)
                                    {
                                        var emptyMagazine = new EmptyMagazineRPC()
                                        {
                                            networkID = playerAspect.networkId,
                                            itemID = itemId,
                                            tick = testTick
                                        };   
                                        if(state.World.IsServer())
                                            RPCHelper.SendEventsToClients<EmptyMagazineRPC>(emptyMagazine,ref state,playerNeedChunkLookup,loadedChunks,entityCommandBuffer,playerAspect.networkId,entity,playerAspect.ghostChunk.ValueRO.GetChunk(),testTick);
                                        else                                          
                                            EntityHelper.CreateEntityWithComponent(entityCommandBuffer,emptyMagazine);  


                                        playerAspect.cooldown.ValueRW.cooldownTick = EntityHelper.AddTime(testTick,0.5f);
                                        break;
                                    }

                                    ammoID = aggregated[playerAspect.playerInputSync.ValueRO.ammoSelectedIndex % aggregated.Length].itemId;
                                    reloadCooldown = weapon.reloadCooldown;

                                    if(state.World.IsServer())
                                    {      
                                        if(EQHelper.PlayerHasTheAmmo(playerAspect.playerInputSync.ValueRO.ammoSelectedItemID,aggregated))
                                            ammoID = playerAspect.playerInputSync.ValueRO.ammoSelectedItemID;
                      
                                        for(int k = 0; k < slots.Length; k++)
                                        {
                                            if(slots[k].itemID == ammoID)
                                            {
                                                var events = EQHelper.SubtractItem(slotsLookup,containersLookup,slots[k].transferData.pos,entity);
                                                EQHelper.SendEvents(entityCommandBuffer, playerAspect.networkId, events);
                                                break;
                                            }
                                        }
                                    }
                                }

                                if(!ItemsAsset.instance.TryGetItem<Ammo>(ammoID,out var ammoItem))
                                    break;

                                var aimPoint = MyTools.ConvertFloat(CalculateAimPoint(rot,weapon)) + currentPosition;
                                testTick.Add(1u);
                                var cooldownTick = EntityHelper.AddTime(testTick,weapon.shootCooldown + reloadCooldown);

                                Debug.Log(state.World.Flags + " shoot cool -> " + cooldownTick.TickIndexForValidTick);
                               


                                playerAspect.cooldown.ValueRW.cooldownTick = cooldownTick;
                                playerAspect.cooldown.ValueRW.startCooldown = NetworkTick.Invalid;
                                uint seed = (uint)testTick.TickIndexForValidTick * 747796405u + 2891336453u;
                                Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);
                                float spread = playerAspect.spread.ValueRO.Spread * 2f;
                                Debug.Log("spread max =>" + spread);
                                spread = random.NextFloat(-1 * spread, spread);                       
                                playerAspect.spread.ValueRW.Spread = math.clamp(playerAspect.spread.ValueRO.Spread + shootingConfig.shootSpread,0,shootingConfig.maxSpread);
                                float offset = ammoItem.bulletOffset;
                                spread -= ammoItem.BulletsSpread / 2f;


                                for(int k = 0 ; k <  ammoItem.bulletCount; k++)
                                {
                                    Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                                    entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = playerAspect.networkId });                            
                                    var baseRot = quaternion.Euler(0, 0, rot);
                                    var spreadRot = quaternion.RotateZ((spread + (offset * k)) * Mathf.Deg2Rad);
                                    var rotation = math.mul(baseRot, spreadRot);
 
                                    LocalTransform lt = new LocalTransform
                                    {
                                        Position = aimPoint,
                                        Rotation = rotation,
                                        Scale = 1f
                                    };
                                    entityCommandBuffer.SetComponent(bullet, lt);
                                    entityCommandBuffer.SetComponent(bullet, new Bullet()
                                    {
                                        bulletID = (uint)(playerAspect.networkId << 16) | (uint)((11 * k + networkTime.ServerTick.TickIndexForValidTick) % 65536),
                                        speed = 4,
                                        damage = 10,
                                        range = 20
                                    });

                                    if (state.World.Flags == WorldFlags.GameServer)
                                    {
                                        entityCommandBuffer.AddComponent(bullet, new GhostChunk().StartValues());
                                        entityCommandBuffer.AddComponent(bullet, new NewChunk());
                                        entityCommandBuffer.AddComponent(bullet, new EntityToHide());
                                        NewBullet bulletComp = SystemAPI.GetComponent<NewBullet>(bullet);

                                        entityCommandBuffer.SetComponent(bullet, bulletComp);
                                    }
                                    else
                                        state.EntityManager.GetComponentObject<SpriteRenderer>(bullet).sprite = ammoItem.BulletSprite;
                                } 


                                var rpc = new PlayerActionRPC()
                                {
                                    networkID = playerAspect.networkId,
                                    itemID = itemId,
                                    tick = testTick
                                };                               

                                playerAspect.playerState.ValueRW.state = PlayerState.shooting;
                                if(state.World.IsServer())
                                {
                                    RPCHelper.SendEventsToClients<PlayerActionRPC>(rpc,ref state,playerNeedChunkLookup,loadedChunks,entityCommandBuffer,playerAspect.networkId,entity,playerAspect.ghostChunk.ValueRO.GetChunk(),testTick);
                                } 
                                else   
                                {                                       
                                    EntityHelper.CreateEntityWithComponent(entityCommandBuffer,rpc); 
                                }                   
                            }
                        }
                    }
                    else if (playerAspect.playerState.ValueRO.state == PlayerState.reloading  || playerAspect.playerState.ValueRO.state == PlayerState.unloading)
                    {
                        Debug.Log("jest input!!!");
                        testTick.Subtract(1);
                        if (playerAspect.input.GetDataAtTick(testTick, out var input2))
                        {
                            uint counter2 = input2.InternalInput.rightButton.Count - input.InternalInput.rightButton.Count;
                            uint counter1 = input2.InternalInput.leftButton.Count - input.InternalInput.leftButton.Count;
                            if(counter2 != 0 || counter1 != 0)
                            {  
                                state.EntityManager.SetComponentData<Cooldown>(entity,new Cooldown(){ cooldownTick = EntityHelper.AddTime(testTick,20) });
                                playerAspect.playerState.ValueRW.state = PlayerState.none;

                                if(state.World.IsServer())
                                    RPCHelper.SendEventsToClientsAndOwner<StopReloadRPC>(ref state,playerNeedChunkLookup,loadedChunks,entityCommandBuffer,playerAspect.networkId,entity,playerAspect.ghostChunk.ValueRO.GetChunk(),testTick);
                            }
                        }
                    }
                }
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
 
    private float2 CalculateAimPoint(float angle,RangedWeapon weapon)
    {
        float2 v = new float2(weapon.aimPoint.x - weapon.gripPoint1.x +0.07f,0);
        float s = math.sin(angle);
        float c = math.cos(angle);
        return new float2(v.x * c - v.y * s , v.x * s + v.y * c);
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

    private float DeltaAngle(float a, float b)
    {
        float delta = b - a;
        return MathF.Abs(Mathf.Atan2(Mathf.Sin(delta), Mathf.Cos(delta)));
    }
}



