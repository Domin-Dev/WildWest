using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


[UpdateInGroup(typeof(GhostInputSystemGroup))]
partial struct PlayerInputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<PlayerInput>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float2 input = float2.zero;

        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.W)) input.y += 1;

        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;


        if (math.lengthsq(input) > 1) input = math.normalize(input);

        foreach ((RefRW<PlayerInput> playerInput, RefRW<PlayerInputSync> playerInputSync) in 
            SystemAPI.Query<RefRW<PlayerInput>, RefRW<PlayerInputSync>>().WithAll<GhostOwnerIsLocal,Simulate>())
        {
            playerInput.ValueRW.movementDir = input;
            playerInputSync.ValueRW.movementDir = input;
        }
    }
}