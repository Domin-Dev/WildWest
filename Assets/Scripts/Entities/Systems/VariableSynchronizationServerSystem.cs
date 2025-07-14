using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;



[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderFirst = true)]
partial struct VariableSynchronizationServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (playerInput,playerInputSync, itemInHandInput, itemInHandInputSync, entity) in
        SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>,RefRO<ItemInHandInput>, RefRW<ItemInHandInputSync>>().WithEntityAccess())
        {
            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
            playerInputSync.ValueRW.sightDirection = playerInput.ValueRO.sightDirection;
            playerInputSync.ValueRW.leftButton = playerInput.ValueRO.leftButton;
            playerInputSync.ValueRW.rightButton = playerInput.ValueRO.rightButton;
            playerInputSync.ValueRW.handRotation = playerInput.ValueRO.handRotation;


            if (itemInHandInputSync.ValueRW.itemInHand != itemInHandInput.ValueRO.itemInHand)
            {
                itemInHandInputSync.ValueRW.itemInHand = itemInHandInput.ValueRO.itemInHand;
                CharacterManager.instance.ChangeItemInHand(itemInHandInputSync.ValueRO.itemInHand, entity, ref state);
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}