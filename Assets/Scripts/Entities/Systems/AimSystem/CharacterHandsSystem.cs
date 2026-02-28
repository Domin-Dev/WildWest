using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(CharacterAimSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct CharacterHandsSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginPresentationEntityCommandBufferSystem.Singleton>();  
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        deltaTime = SystemAPI.Time.DeltaTime;


        foreach ((RefRW<Hands> hands,RefRO<AimRotation> aimRotation, RefRW<Character> character) in SystemAPI.Query<RefRW<Hands>,RefRO<AimRotation>,RefRW<Character>>().WithNone<NewPlayerTag>().WithAll<Simulate>())
        {
            LocalTransform localSideHand = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.side);
            LocalToWorld worldMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform localItem = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInHand);
            LocalTransform localMain = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.main);
            
            float rot = aimRotation.ValueRO.angle;   
            UpdateAimSystem(rot, ref localSideHand, ref localItem, ref localMain, hands);

            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.main, localMain);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.side, localSideHand);
            state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.itemInHand, localItem);

            UpdateDirectionIndex(rot,character, ref state);
        }


        foreach ((RefRO<PlayerActionRPC> action,Entity rpc) in SystemAPI.Query<RefRO<PlayerActionRPC>>().WithEntityAccess())
        {
            
            foreach((RefRO<Hands> hands,RefRO<GhostOwner> ghostOwner) in SystemAPI.Query<RefRO<Hands>,RefRO<GhostOwner>>().WithAll<Player,Simulate>())
            {
                if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;

                LocalToWorld aimpoint = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.aimPoint);
                Sounds.instance.Shot();
                EntitySpawner.instance.SpawnEntityPrefab(2, aimpoint.Position, aimpoint.Rotation);
                EntitySpawner.instance.SpawnParticle(0, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.05f, 0f, 0f)), quaternion.identity);
                EntitySpawner.instance.SpawnParticle(1, aimpoint.Position + math.rotate(aimpoint.Rotation, new float3(0.01f, 0f, 0f)), aimpoint.Rotation);          
                break;
            }
            entityCommandBuffer.DestroyEntity(rpc);
        }
    }

    private void UpdateAimSystem(float angle,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands, float maxDeltaTime = 0.04f)
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

        var rotationDifference = math.abs(math.angle(localMain.Rotation, mainTargetRotation));
        localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, math.min(deltaTime, maxDeltaTime) * 20);
        localSideHand.Rotation = math.slerp(localSideHand.Rotation, sideTargetRotation, math.min(deltaTime, maxDeltaTime) * 8);

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

    
}


