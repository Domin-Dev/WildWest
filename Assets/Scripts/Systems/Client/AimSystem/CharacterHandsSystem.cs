using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct CharacterHandsSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
    }




            //         if (hands.ValueRW.actionStatus != 0)
            //         {
            //             transform.Position = hands.ValueRO.targetPosition;
            //             transform.Rotation = hands.ValueRO.targetRotation;
            //         }

            //         SetActionStatus(ref state, 2, hands, transform.Rotation, math.normalize(math.mul(addedRotation, transform.Rotation)), transform.Position, transform.Position - new float3(0.06f, 0, 0));

            //         if (state.World.Flags != WorldFlags.GameServer)
            //         {
            //             Sounds.instance.Shot();
            //             EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
            //             EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)), quaternion.identity);
            //             EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation);
            //         }

            //         if (lastAction.HasValue) entityCommandBuffer.SetComponent(entity, lastAction.Value);
            //         continue;
            //     }
            // }

            // if (hands.ValueRO.actionStatus != 0)
            // {
            //     ActionUpdate(hands, playerAspect.player, ref state);
            // }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        deltaTime = SystemAPI.Time.DeltaTime;


        

        foreach ((RefRO<PlayerActionRPC> action,Entity rpc) in SystemAPI.Query<RefRO<PlayerActionRPC>>().WithEntityAccess())
        {      
            foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<Velocity2D> vel, Entity e) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<Velocity2D>>().WithAll<Player>().WithEntityAccess())
            {
                if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;

                Sounds.instance.Shot();
                
                
                LocalToWorld worldPosMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.mainhand);
                quaternion addedRotation = quaternion.Euler(0, 0, math.radians(70));


                LocalToWorld aimpoint = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.aimPoint);
                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
                
                if (hands.ValueRW.actionStatus != 0)
                {
                    transform.Position = hands.ValueRO.targetPosition;
                    transform.Rotation = hands.ValueRO.targetRotation;
                }

                 
                SetActionStatus(ref state, 2, hands, transform.Rotation, math.normalize(math.mul(addedRotation, transform.Rotation)), transform.Position, transform.Position - new float3(0.06f, 0, 0));


                EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
                EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)), quaternion.identity,new NewParticles()
                {
                    target = hands.ValueRO.aimPoint,
                    offset = new float3(0.05f, 0f, 0f)
                });
                EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation, new NewParticles()
                {
                    target = hands.ValueRO.aimPoint,
                    offset = new float3(0.01f,0f,0f)
                });  
                break;
            }
            entityCommandBuffer.DestroyEntity(rpc);
        }
       

        foreach ((RefRW<Hands> hands,RefRO<AimRotation> aimRotation, RefRW<Character> character) in SystemAPI.Query<RefRW<Hands>,RefRO<AimRotation>,RefRW<Character>>().WithNone<NewPlayerTag>())
        {
            LocalTransform localSideHand = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
            LocalToWorld worldMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform localItem = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);
            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.main);
            

            if (hands.ValueRO.actionStatus != 0)
            {
                ActionUpdate(hands, ref state);
            }
            float rot = aimRotation.ValueRO.angle;   
            UpdateAimSystem(rot, ref localSideHand, ref localItem, ref localMain, hands);

            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSideHand);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, localItem);

            UpdateDirectionIndex(rot,character, ref state);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }       

    private void UpdateAimSystem(float angle,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands, float maxDeltaTime = 0.05f)
    {
        quaternion mainTargetRotation;
        quaternion sideTargetRotation;
        if (math.abs(angle) > leftSide)
        {
            float3 euler = math.Euler(localMain.Rotation,math.RotationOrder.XYZ);
            euler.x = math.radians(180);
            localMain.Rotation = quaternion.EulerXYZ(euler);
            var p = localItem.Position;
            p.z = -0.0001f;
            localItem.Position = p;
            
            sideTargetRotation = quaternion.Euler(0, 0, angle - math.radians(90));
            angle = -angle;
            mainTargetRotation = quaternion.Euler(math.radians(180), 0, angle);
        }
        else
        {
            float3 euler = math.Euler(localMain.Rotation,math.RotationOrder.XYZ);
            euler.x = 0;
            localMain.Rotation = quaternion.EulerXYZ(euler);
            var p = localItem.Position;
            p.z = 0.0001f;
            localItem.Position = p;
          
            sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(90));
            mainTargetRotation = quaternion.Euler(0, 0, angle);
        }


          //  localMain.Rotation = mainTargetRotation;
          //  localSideHand.Rotation = sideTargetRotation;
        localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, math.min(deltaTime, maxDeltaTime) * 22);
       localSideHand.Rotation = math.slerp(localSideHand.Rotation, sideTargetRotation, math.min(deltaTime, maxDeltaTime) * 10);

        if (math.Euler(localMain.Rotation).z > 0) localMain.Position.z = 0.0011f;
        else localMain.Position.z = -0.001f;
    }
    public static void GetHandsRotation(float angle,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands)
    {
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

        localMain.Rotation =  mainTargetRotation;
        localSideHand.Rotation = sideTargetRotation;

        if (math.Euler(localMain.Rotation).z > 0) localMain.Position.z = 0.0011f;
        else localMain.Position.z = -0.001f;
    }    
    private void UpdateDirectionIndex(float angle, RefRW<Character> character, ref SystemState state)
    {
        int newDirIndex = PlayersInputsServiceClientSystem.GetDirectionIndex(angle);
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
    public void ActionUpdate(RefRW<Hands> hands, ref SystemState state)
    {
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
        hands.ValueRW.elapsedTime += deltaTime;


        float t = math.clamp(hands.ValueRO.elapsedTime / GetActionTime(hands.ValueRO.actionStatus), 0f, 1f);
        localTransform.Rotation = math.slerp(localTransform.Rotation, hands.ValueRO.targetRotation, t);
        localTransform.Position = math.lerp(localTransform.Position, hands.ValueRO.targetPosition, t);

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


