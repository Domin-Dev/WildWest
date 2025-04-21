using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct PlayersInputsServiceServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<Player>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInput, playerInputSync, player, velocity,entity)
         in SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>, RefRO<Player>,RefRW<Velocity2D>>().WithAll<Simulate>().WithEntityAccess())
        {
            Debug.Log("dzial!!!!!!!");
            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDir;
            velocity.ValueRW.Value = playerInput.ValueRO.movementDir * SystemAPI.Time.DeltaTime * player.ValueRO.speed;
            bool shouldBeChanged = !(playerInput.ValueRO.movementDir.x == 0 && playerInput.ValueRO.movementDir.y == 0);
            state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
        }
    }
}