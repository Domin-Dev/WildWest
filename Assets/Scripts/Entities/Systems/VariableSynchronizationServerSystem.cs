using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;



[UpdateAfter(typeof(NewPlayerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct VariableSynchronizationServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (playerInput,playerInputSync) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>>())
        {
            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
            playerInputSync.ValueRW.sightDirection = playerInput.ValueRO.sightDirection;
            playerInputSync.ValueRW.leftButton = playerInput.ValueRO.leftButton;
            playerInputSync.ValueRW.rightButton = playerInput.ValueRO.rightButton;
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}