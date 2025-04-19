using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderFirst = true)]
partial struct PlayersInputsServiceSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (playerInput,player,velocity,entity)
         in SystemAPI.Query<RefRO<PlayerInput>,RefRO<Player>, RefRW<Velocity2D>>().WithAll<Simulate,GhostOwnerIsLocal>().WithEntityAccess())
        {
            velocity.ValueRW.Value = playerInput.ValueRO.movementDir * deltaTime * player.ValueRO.speed;
            bool shouldBeChanged = !(velocity.ValueRO.Value.x == 0 && velocity.ValueRO.Value.y == 0);
            state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
        }
    }
}