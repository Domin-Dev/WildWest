
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

        foreach ((RefRW <LocalTransform> transform, RefRW < PhysicsVelocity > velocity, RefRO <Player> player ) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRO<Player>>())
        {
            float2 float2 = input * player.ValueRO.speed * deltaTime;
            velocity.ValueRW.Linear = new float3(float2.x, float2.y, 0f); 
        } 
    }

}



