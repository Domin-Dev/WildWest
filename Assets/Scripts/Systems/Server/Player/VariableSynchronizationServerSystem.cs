using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderFirst = true)]
partial struct VariableSynchronizationServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>(true);
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);

        var loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach (var (playerInput,playerInputSync,owner,ghostChunk, entity) in
        SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>,RefRO<GhostOwner>,RefRO<GhostChunk>>().WithEntityAccess())
        {
            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
            playerInputSync.ValueRW.sightDirection = playerInput.ValueRO.sightDirection;
            playerInputSync.ValueRW.leftButton = playerInput.ValueRO.leftButton;
            playerInputSync.ValueRW.rightButton = playerInput.ValueRO.rightButton;


            int NewSlotInHand = playerInput.ValueRO.slotInHand;
            if (NewSlotInHand != playerInputSync.ValueRO.slotInHand)
            {
                playerInputSync.ValueRW.slotInHand = NewSlotInHand;
                var from = new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,NewSlotInHand);
                var to = new SlotPosition(EquipmentConfig.itemInHand_ContainerIndex,0);
                EQHelper.Clone(ecb,from,to,barsLookup,slotsLookup,containersLookup,entity);
                RPCHelper.SendEventsToClients<NewItemInHandRPC>(ref state,playerNeedChunkLookup,loadedChunks,ecb,owner.ValueRO.NetworkId,ghostChunk.ValueRO.GetChunk(),tick);
            }
      }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}