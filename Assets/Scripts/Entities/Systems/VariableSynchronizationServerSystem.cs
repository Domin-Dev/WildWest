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

        foreach (var (playerInput,playerInputSync, itemInHandInput, itemInHandInputSync) in
        SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>,RefRO<ItemInHandInput>, RefRW<ItemInHandInputSync>>())
        {
            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
            playerInputSync.ValueRW.sightDirection = playerInput.ValueRO.sightDirection;
            playerInputSync.ValueRW.leftButton = playerInput.ValueRO.leftButton;
            playerInputSync.ValueRW.rightButton = playerInput.ValueRO.rightButton;

            itemInHandInputSync.ValueRW.itemInHand = itemInHandInput.ValueRO.itemInHand;
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}