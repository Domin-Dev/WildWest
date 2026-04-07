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
    private BufferLookup<PlayerContainers> containersLookup;

    private int simulationTickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();

        playerNeedChunkLookup = state.GetBufferLookup<PlayersNeedChunk>(true);
        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>(true);
        
        simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }


  
    public void OnUpdate(ref SystemState state)
    {
        playerNeedChunkLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);

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
                                Debug.Log("shoot!!! " + testTick.TickIndexForValidTick +  " cool = " +  playerAspect.cooldown.ValueRO.cooldownTick.TickIndexForValidTick);

                                if(!EQHelper.TryGetPlayerContainer(containersLookup,entity,EquipmentConfig.itemInHand_ContainerIndex,out var playerContainer))
                                    break;

                                if(!EQHelper.TryGetBufferIndex(slotsLookup,0,playerContainer.Value.entity,out int itemId, out int bufferIndex))
                                    break;

                                if(!ItemsAsset.instance.TryGetItem<RangedWeapon>(itemId,out var weapon))
                                    break;


                                int ammoID = -1;
                                if(weapon.hasMagazine)
                                {
                                   Debug.Log("z"); 
                                }
                                else
                                {
                                    var slots = EQHelper.TryFindItemWithTag(ref state,slotsLookup,containersLookup,entity,weapon.ammoTagID,out var aggregated,out int counter);
                                    
                                    if(counter == 0) break;
                                    ammoID = aggregated[playerAspect.playerInputSync.ValueRO.ammoSelectedIndex % aggregated.Length].itemId;
                                    if(state.World.IsServer())
                                    {                                     
                                        for(int k = 0; k < slots.Length; k++)
                                        {
                                            if(slots[k].itemID == ammoID)
                                            {
                                                var events = EQHelper.SubtractItem(ref state,slotsLookup,containersLookup,slots[k].transferData.pos,entity);
                                                EQHelper.SendEvents(entityCommandBuffer, playerAspect.networkId, events);
                                                break;
                                            }
                                        }
                                    }
                                }



                                var aimPoint = MyTools.ConvertFloat(CalculateAimPoint(rot,weapon)) + currentPosition;

                                testTick.Add(1u);
                                var cooldownTick = testTick;

                                cooldownTick.Add((uint)(simulationTickRate * weapon.cooldown));
                                playerAspect.cooldown.ValueRW.cooldownTick = cooldownTick;


                                uint seed = (uint)testTick.TickIndexForValidTick * 311u; //* 747796405u + 2891336453u;
                                Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);
                                float spread = weapon.shotSpread;
                                float offset = weapon.bulletOffset;
                                float value = 0f;//random.NextFloat((-1 * spread) + weapon.bulletSpread,spread);
                                

                                for(int k = 0 ; k < weapon.bulletCount; k++)
                                {
                                    Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                                    entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = playerAspect.networkId });
                                    

                                    var baseRot = quaternion.Euler(0, 0, rot);
                                    var spreadRot = quaternion.RotateZ(k * 2 * Mathf.Deg2Rad);
                                    var rotation = math.mul(baseRot, spreadRot);

                                    //var rotation = quaternion.Euler(0, 0, rot + (currentSpread * Mathf.Deg2Rad));
                                    //LocalTransform lt = LocalTransform.FromPosition(aimPoint).Rotate(rotation);
                                    LocalTransform lt = new LocalTransform
                                    {
                                        Position = aimPoint + new float3(0,-0.05f * k,0),
                                        Rotation = rotation,
                                        Scale = 1f
                                    };
                                    entityCommandBuffer.SetComponent(bullet, lt);

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
                                        if(ItemsAsset.instance.TryGetItem(ammoID,out var item))
                                            state.EntityManager.GetComponentObject<SpriteRenderer>(bullet).sprite = item.GetWorldSprite;
                                    }
                                } 

                                var rpc = new PlayerActionRPC()
                                {
                                    networkID = playerAspect.networkId,
                                    itemID = itemId
                                };                               


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
          
                }
            }
            playerAspect.aimRotation.ValueRW.angle = rot;
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
}
