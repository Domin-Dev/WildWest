using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
 partial struct PlayersInputsServiceClientSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<PlayerInputSync, Character, Simulate>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInput,character,entity)
         in SystemAPI.Query<RefRO<PlayerInputSync>, RefRW<Character>>().WithAll<Simulate>().WithEntityAccess())
        {
            bool shouldBeChanged = !(playerInput.ValueRO.movementDir.x == 0 && playerInput.ValueRO.movementDir.y == 0);

            if (character.ValueRW.isMove)
            {
                if (!shouldBeChanged)
                {
                    ResetAnim(character, ref state);
                }
            }
            else
            {
                if (shouldBeChanged)
                {
                    StartAnim(character, ref state);
                }
            }
            if (shouldBeChanged) UpdateDirectionIndex(playerInput.ValueRO.movementDir, character, ref state);
        }
    }

    private void ResetAnim(RefRW<Character> character, ref SystemState state)
    {
        LocalTransform body = state.EntityManager.GetComponentData<LocalTransform>(character.ValueRO.body);
        LocalTransform head = state.EntityManager.GetComponentData<LocalTransform>(character.ValueRO.headParent);

        body.Rotation = quaternion.identity;
        head.Rotation = quaternion.identity;
        head.Position = new float3(0, CharacterAnimationSystem.headOffsetY, 0);

        character.ValueRW.directionBody = character.ValueRO.directionHead;
        CharacterAimSystem.SetDirection(character.ValueRO.body, character.ValueRO.directionHead, ref state);

        state.EntityManager.SetComponentData<LocalTransform>(character.ValueRO.body, body);
        state.EntityManager.SetComponentData<LocalTransform>(character.ValueRO.headParent, head);
        character.ValueRW.isMove = false;
    }
    private void StartAnim(RefRW<Character> character, ref SystemState state)
    {
        character.ValueRW.startAnim = (float)SystemAPI.Time.ElapsedTime;
        character.ValueRW.isMove = true;
    }

    public static int GetDirectionIndex(float2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle += 180;

        if (angle >= 20 && angle <= 160) return 0;
        else if (angle > 160 && angle < 200) return 2;
        else if (angle >= 200 && angle <= 340) return 1;
        else return 3;
    }

    private void UpdateDirectionIndex(float2 dir, RefRW<Character> character, ref SystemState state)
    {
        int newDirIndex = GetDirectionIndex(dir);
        if (newDirIndex != character.ValueRO.directionBody)
        {
            character.ValueRW.directionBody = newDirIndex;
            CharacterAimSystem.SetDirection(character.ValueRO.body, newDirIndex, ref state);
        }
    }
}