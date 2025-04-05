using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

partial struct PlayerMovementSystem : ISystem
{


    private static float leftSide = math.PI / 2f;
    static float x = 1;
    public void OnUpdate(ref SystemState state)
    {
        float2 input = float2.zero;

        if (Input.GetKey(KeyCode.W)) input.y += 1;



        if (Input.GetKeyDown(KeyCode.E))
        {
            if (x == 1) x = 20;
            else x = 1;
        }

        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;

        if (math.lengthsq(input) > 1) input = math.normalize(input);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (velocity, player, character, entity)
         in SystemAPI.Query<RefRW<Velocity2D>, RefRO<Player>, RefRW<Character>>().WithEntityAccess())
        {
            float2 vector = input * player.ValueRO.speed * deltaTime * x;
            velocity.ValueRW.Value = vector;
            bool shouldBeChanged = !(vector.x == 0 && vector.y == 0);

            state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
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
            if(shouldBeChanged) UpdateDirectionIndex(vector, character, ref state);
        }
    }

    private void ResetAnim(RefRW<Character> character,ref SystemState state)
    {
        LocalTransform body = state.EntityManager.GetComponentData<LocalTransform>(character.ValueRO.body);
        LocalTransform head = state.EntityManager.GetComponentData<LocalTransform>(character.ValueRO.headParent);
        
        body.Rotation = quaternion.identity;
        head.Rotation = quaternion.identity;
        head.Position = new float3(0,CharacterAnimationSystem.headOffsetY, 0);

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

    private  void UpdateDirectionIndex(float2 dir, RefRW<Character> character, ref SystemState state)
    {
        int newDirIndex = GetDirectionIndex(dir);
        if (newDirIndex != character.ValueRO.directionBody)
        {
            character.ValueRW.directionBody = newDirIndex;
            CharacterAimSystem.SetDirection(character.ValueRO.body, newDirIndex, ref state);
        }
    }


}