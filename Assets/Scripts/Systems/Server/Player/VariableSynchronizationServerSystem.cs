using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Assertions.Must;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderFirst = true)]
partial struct VariableSynchronizationServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;
    private BufferLookup<LinkedContainers> linkedContainers;




    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>(true);
        linkedContainers = SystemAPI.GetBufferLookup<LinkedContainers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);
        linkedContainers.Update(ref state);

        var loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach (var (playerInput,playerInputSync,owner,ghostChunk,cooldown, entity) in
        SystemAPI.Query<RefRO<PlayerInput>, RefRW<PlayerInputSync>,RefRO<GhostOwner>,RefRO<GhostChunk>,RefRW<Cooldown>>().WithNone<NewPlayerTag>().WithEntityAccess())
        {
            int newAmmoIndex = playerInput.ValueRO.ammoSelectedIndex;
            bool newAmmo = newAmmoIndex != playerInputSync.ValueRO.ammoSelectedIndex;

            playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
            playerInputSync.ValueRW.sightDirection = playerInput.ValueRO.sightDirection;
            playerInputSync.ValueRW.leftButton = playerInput.ValueRO.leftButton;
            playerInputSync.ValueRW.rightButton = playerInput.ValueRO.rightButton;


            int NewSlotInHand = Mathf.Clamp(playerInput.ValueRO.slotInHand,0,9); 
            if (NewSlotInHand != playerInputSync.ValueRO.slotInHand && ghostChunk.ValueRO.HasChunk())
            {               
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
            

            if(newAmmo || playerInput.ValueRO.reloadButton.IsSet || playerInput.ValueRO.unloadButton.IsSet)
            {
                playerInputSync.ValueRW.ammoSelectedIndex = newAmmoIndex;
                EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,entity,new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,NewSlotInHand), out var slot,out int bufferIndex, out Entity containerEntity);
                if(slot.HasValue && ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
                {
                    if(item.hasMagazine && playerInput.ValueRO.reloadButton.IsSet && (!cooldown.ValueRO.cooldownTick.IsValid || tick.IsNewerThan(cooldown.ValueRO.cooldownTick) ||
                    (cooldown.ValueRO.startCooldown.IsValid && cooldown.ValueRO.startCooldown.IsNewerThan(tick))))
                    {
                        Debug.Log("liczba ammo ! to " + EQHelper.CountItemsInLinkedContainer(slotsLookup,linkedContainers,containerEntity,NewSlotInHand));
                        if(EQHelper.CountItemsInLinkedContainer(slotsLookup,linkedContainers,containerEntity,NewSlotInHand) < item.magazineCapacity)
                        {
                            var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,entity,item.ammoTagID,out int counter);         
                            if(ammo.Length > 0)
                            {
                                var selectedAmmo = playerInput.ValueRO.ammoSelectedIndex % ammo.Length;
                                var ammoID = ammo[selectedAmmo].itemId;
                                playerInputSync.ValueRW.ammoSelectedIndex = selectedAmmo;
                                Debug.Log(" reload new RPC!!!");
                                RPCHelper.SendEventsToClientsAndOwner<ReloadRPC>(new ReloadRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,ref state,playerNeedChunkLookup,loadedChunks,ecb,owner.ValueRO.NetworkId,entity,ghostChunk.ValueRO.GetChunk(),tick);
                                cooldown.ValueRW.cooldownTick = EntityHelper.AddTime(tick,4);
                            }
                        }
                    }
                    else if(!item.hasMagazine && newAmmo)
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
                                cooldown.ValueRW.cooldownTick = EntityHelper.AddTime(tick,4);
                            }
                        }
                    }
                    else if (playerInput.ValueRO.unloadButton.IsSet)
                    {
                        RPCHelper.SendEventsToClientsAndOwner<UnloadRPC>(new UnloadRPC(){ weaponID =  slot.Value.itemId} ,ref state,playerNeedChunkLookup,loadedChunks,ecb,owner.ValueRO.NetworkId,entity,ghostChunk.ValueRO.GetChunk(),tick);
                    }
                }
            }
      }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}