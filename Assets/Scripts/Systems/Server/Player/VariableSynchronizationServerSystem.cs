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
        SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>,RefRO<GhostOwner>,RefRO<GhostChunk>>().WithNone<NewPlayerTag>().WithEntityAccess())
        {
            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
            playerInputSync.ValueRW.sightDirection = playerInput.ValueRO.sightDirection;
            playerInputSync.ValueRW.leftButton = playerInput.ValueRO.leftButton;
            playerInputSync.ValueRW.rightButton = playerInput.ValueRO.rightButton;


            int NewSlotInHand = playerInput.ValueRO.slotInHand;
            if (NewSlotInHand != playerInputSync.ValueRO.slotInHand && ghostChunk.ValueRO.HasChunk())
            {               
                Debug.Log("mmmmmmmmmmmmmmmmmmm  " + " "+ ghostChunk.ValueRO.GetChunk());
                var from = new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,NewSlotInHand);
                var to = EquipmentConfig.itemInHand_SlotPosition;
                EQHelper.Clone(from,to,barsLookup,slotsLookup,containersLookup,entity,out var newSlot,out var newBarData);
                int itemID = newSlot.HasValue ? newSlot.Value.itemId : -1;
                
                bool result;                    
                if(playerInputSync.ValueRO.slotInHand != -1)
                   result = RPCHelper.SendEventsToClients<NewItemInHandRPC>(new NewItemInHandRPC(){itemID = itemID },ref state,playerNeedChunkLookup,loadedChunks,ecb,owner.ValueRO.NetworkId,entity,ghostChunk.ValueRO.GetChunk(),tick);     
                else
                   result = RPCHelper.SendEventsToClientsAndOwner<NewItemInHandRPC>(new NewItemInHandRPC(){itemID = itemID },ref state,playerNeedChunkLookup,loadedChunks,ecb,owner.ValueRO.NetworkId,entity,ghostChunk.ValueRO.GetChunk(),tick);     

                if(result)
                {
                    playerInputSync.ValueRW.slotInHand = NewSlotInHand;
                    state.EntityManager.SetComponentData<Cooldown>(entity,new Cooldown(){ cooldownTick = EntityHelper.AddTime(tick,2)});
                }
            }
            
            int newAmmoIndex = playerInput.ValueRO.ammoSelectedIndex;
            if(newAmmoIndex != playerInputSync.ValueRO.ammoSelectedIndex)
            {
                playerInputSync.ValueRW.ammoSelectedIndex = newAmmoIndex;
                EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,entity,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex);
                if(slot.HasValue && ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
                {
                    var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,entity,item.ammoTagID,out int counter);
                    if(ammo.Length > 0)
                    {
                        var selectedAmmo = playerInput.ValueRO.ammoSelectedIndex % ammo.Length;
                        var ammoID = ammo[selectedAmmo].itemId;
                        if(playerInputSync.ValueRW.ammoSelectedItemID != ammoID)
                        {
                            playerInputSync.ValueRW.ammoSelectedIndex = selectedAmmo;
                            playerInputSync.ValueRW.ammoSelectedItemID = ammoID;
                            RPCHelper.SendEventsToClientsAndOwner<NewAmmoSelectedRPC>(new NewAmmoSelectedRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,ref state,playerNeedChunkLookup,loadedChunks,ecb,owner.ValueRO.NetworkId,entity,ghostChunk.ValueRO.GetChunk(),tick);
                            state.EntityManager.SetComponentData<Cooldown>(entity,new Cooldown(){ cooldownTick = EntityHelper.AddTime(tick,4)});
                        }
                    }
                }
            }

      }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}