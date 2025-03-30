
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct PlayerMovementSystem : ISystem
{


    private static float leftSide = math.PI / 2f;
    public void OnUpdate(ref SystemState state)
    {
        float2 input = float2.zero;
        if (Input.GetKey(KeyCode.W)) input.y += 1;
        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;

        if (math.lengthsq(input) > 1) input = math.normalize(input);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (velocity, player, entity)
         in SystemAPI.Query<RefRW<Velocity2D>, RefRO<Player>>().WithEntityAccess())
        {
            float2 vector = input * player.ValueRO.speed * deltaTime;
            velocity.ValueRW.Value = vector;
            bool shouldBeChanged = !(vector.x == 0 && vector.y == 0);
            state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
        } 
    }

}



