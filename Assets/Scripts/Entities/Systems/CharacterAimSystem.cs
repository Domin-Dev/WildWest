using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]

partial struct CharacterAimSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;
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

        foreach ((RefRO<PlayerInputSync> playerInput, RefRW<Hands> hands, RefRW<Character> character, RefRO<LocalToWorld> worldPos , RefRO<GhostOwner> ghostOwner, Entity entity) 
        in SystemAPI.Query < RefRO<PlayerInputSync>, RefRW<Hands>, RefRW<Character>,RefRO<LocalToWorld>, RefRO<GhostOwner>>().WithNone<NewPlayerTag>().WithAll<Simulate>().WithEntityAccess())
        {
            if (!networkTime.IsFirstTimeFullyPredictingTick) continue;

            if (hands.ValueRO.actionStatus != 0)
            {
                ActionUpdate(hands, ref state);
                continue;
            }

            
            if (playerInput.ValueRO.leftButton.IsSet)
            {

                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
                quaternion addedRotation = quaternion.Euler(0, 0, math.radians(-110));
                hands.ValueRW.targetRotation = math.normalize(math.mul(addedRotation, transform.Rotation));
                hands.ValueRW.lastPosition = transform.Position;
                hands.ValueRW.targetPosition = transform.Position + new float3(0.06f, 0, 0);
                hands.ValueRW.actionStatus = 1;
            }
            if (playerInput.ValueRO.rightButton.IsSet)
            {
                if(state.World.Flags == WorldFlags.GameServer || state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                { 
                    Entity bullet = state.EntityManager.Instantiate(entitiesReferences.bulletEntity);
                    entityCommandBuffer.SetComponent(bullet, new GhostOwner() { NetworkId = ghostOwner.ValueRO.NetworkId });
                    entityCommandBuffer.SetComponent(bullet, LocalTransform.FromPosition(worldPos.ValueRO.Position));

                    if (state.World.Flags == WorldFlags.GameServer)
                    {
                        entityCommandBuffer.AddComponent(bullet, new EntityToHide());
                        Bullet bulletComp = SystemAPI.GetComponent<Bullet>(bullet);
                        bulletComp.time = 20;
                        entityCommandBuffer.SetComponent(bullet, bulletComp);
                    }
                }


                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
                LocalToWorld worldPosMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.mainhand);

                quaternion addedRotation = quaternion.Euler(0, 0, math.radians(70));
                hands.ValueRW.targetRotation = math.normalize(math.mul(addedRotation, transform.Rotation));
                hands.ValueRW.lastPosition = transform.Position;
                hands.ValueRW.targetPosition = transform.Position - new float3(0.06f, 0, 0);
                hands.ValueRW.actionStatus = 2;
                EntitySpawner.instance.SpawnParticle(0, new float3(0, 0.1f, 0) + worldPosMainHand.Position);
            }
            

       
            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.main);
            LocalTransform localSide = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
            LocalToWorld localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform local = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);

            float3 currentPosition = localToWorld.Position; 
            float2 direction = playerInput.ValueRO.sightDirection - new float2(currentPosition.x,currentPosition.y);


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

            localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, deltaTime * 10f);
            localSide.Rotation = math.slerp(localSide.Rotation, sideTargetRotation, deltaTime * 2f);

            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSide);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, local);
            UpdateDirectionIndex(new float2(direction.x,direction.y), character,ref state);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    public void ActionUpdate(RefRW<Hands> hands, ref SystemState state)
    {
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);

        localTransform.Rotation = math.slerp(localTransform.Rotation, hands.ValueRO.targetRotation, deltaTime * 15f);
        localTransform.Position = math.lerp(localTransform.Position, hands.ValueRO.targetPosition, deltaTime * 20);


        if (MyTools.EqualFloat3(localTransform.Position,hands.ValueRO.targetPosition,0.01f) && MyTools.EqualQuaternions(localTransform.Rotation,hands.ValueRO.targetRotation,0.98f))
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
        state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.mainhand, localTransform);
    }

    private void UpdateDirectionIndex(float2 dir, RefRW<Character> character, ref SystemState state)
    {
        int newDirIndex = PlayersInputsServiceClientSystem.GetDirectionIndex(dir);
        if (newDirIndex != character.ValueRO.directionHead)
        {
            character.ValueRW.directionHead = newDirIndex;
            SetDirection(character.ValueRO.head, newDirIndex, ref state);
            if(!character.ValueRO.isMove)
            {
                SetDirection(character.ValueRO.body, newDirIndex, ref state);
                character.ValueRW.directionBody = newDirIndex;
            }
        }
    }

    public static void SetDirection(Entity entity,int newIndex, ref SystemState state)
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
