using Unity.Entities;
using Unity.Entities.Build;
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


        foreach ((RefRW<Hands> hands,RefRO<AimRotation> aimRotation) in SystemAPI.Query<RefRW<Hands>,RefRO<AimRotation>>().WithNone<NewPlayerTag>().WithAll<Simulate>())
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
        }
    }


    private void UpdateAimSystem(float angle,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands)
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

        localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, deltaTime * 20);
        localSideHand.Rotation = math.slerp(localSideHand.Rotation, sideTargetRotation, deltaTime * 8);

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

    
}


