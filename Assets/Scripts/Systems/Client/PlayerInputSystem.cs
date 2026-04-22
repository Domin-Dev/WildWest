using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


[UpdateInGroup(typeof(GhostInputSystemGroup),OrderFirst = true)]
partial struct PlayerInputSystem : ISystem
{   
    public static event Action<int,InventorySlot?> onNewSlotInHand;




    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> containersLookup;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<PlayerInput>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>(true);

    }
    public void OnUpdate(ref SystemState state)
    {
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        float2 input = (float2)InputManager.i.move.ReadValue<Vector2>();
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        bool left = InputManager.i.mainAction.inProgress;
        bool right = InputManager.i.sideAction.inProgress;
        bool reloadButton = InputManager.i.reload.inProgress;
        bool unloadButton = InputManager.i.unload.inProgress;
        if (math.lengthsq(input) > 1) input = math.normalize(input);


        float3 target = (float3)MyTools.GetMouseWorldPosition();
        float2 sightDirection = new float2(target.x,target.y);

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (!WindowsManager.instance.CloseOpenWindows())
        //        WindowsManager.instance.LoadScene(11);
        //}
        if (InputManager.i.playerList.triggered && !ChatManager.instance.isChatting)
        {
            WindowsManager.instance.LoadScene(9);
        }

        

        foreach ((RefRW<PlayerInput> playerInput, RefRW<PlayerInputSync> playerInputSync , RefRW<Hands> hands, RefRO<GhostOwner> owner,RefRW<Cooldown> cooldown, Entity playerEntity) in 
            SystemAPI.Query<RefRW<PlayerInput>, RefRW<PlayerInputSync>, RefRW<Hands>,RefRO<GhostOwner>,RefRW<Cooldown>>().WithAll<GhostOwnerIsLocal,Simulate>().WithNone<NewPlayerTag>().WithEntityAccess())
        {
            if(!UIManager.instance.WindowsAreClosed)
            {
                playerInput.ValueRW.leftButton = default;
                playerInputSync.ValueRW.leftButton = default;

                playerInput.ValueRW.rightButton = default;
                playerInputSync.ValueRW.rightButton = default;

                playerInput.ValueRW.movementDirection = float2.zero;
                playerInputSync.ValueRW.movementDir = float2.zero;
                
                playerInput.ValueRW.reloadButton = default;
                playerInput.ValueRW.unloadButton = default;

                continue;
            }

            playerInput.ValueRW.movementDirection = input;
            playerInputSync.ValueRW.movementDir = input;

            playerInput.ValueRW.sightDirection = sightDirection;
            playerInputSync.ValueRW.sightDirection = sightDirection;

            int newSlot = InputManager.i.GetNextSlotInHand(playerInput.ValueRO.slotInHand);
            int newAmmoIndex = InputManager.i.GetNextAmmoIndex(playerInput.ValueRO.ammoSelectedIndex);

            if(playerInputSync.ValueRO.slotInHand != newSlot)
            {
                UpdateItemInHand(slotsLookup,containersLookup,playerEntity,newSlot);
                playerInput.ValueRW.slotInHand = newSlot;
                playerInputSync.ValueRW.slotInHand = newSlot;
                
                if(cooldown.ValueRO.cooldownTick.IsValid && tick.IsNewerThan(cooldown.ValueRO.cooldownTick))
                    cooldown.ValueRW.startCooldown =  EntityHelper.AddTime(tick,1);
                cooldown.ValueRW.cooldownTick = EntityHelper.AddTime(tick,5);
                EntityHelper.CreateEntityWithComponent<NewItemInHandRPC>(ecb, new NewItemInHandRPC() { networkID = owner.ValueRO.NetworkId  });
            }

            if(playerInputSync.ValueRO.ammoSelectedIndex != newAmmoIndex)
            {
                int ammoID = -1;
                playerInput.ValueRW.ammoSelectedIndex = newAmmoIndex;
                playerInputSync.ValueRW.ammoSelectedIndex = newAmmoIndex;
                
                if(EQHelper.TryGetPlayerContainer(containersLookup,playerEntity,EquipmentConfig.itemInHand_ContainerIndex,out var playerContainer))
                {                         
                    EQHelper.TryGetBufferIndex(slotsLookup,0,playerContainer.Value.entity,out InventorySlot? slot, out int bufferIndex);
                    if(slot.HasValue && ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
                    {
                        var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,playerEntity,item.ammoTagID,out int counter);
                        if(ammo.Length > 0)
                        {
                            newAmmoIndex =  newAmmoIndex % ammo.Length;
                            ammoID = ammo[newAmmoIndex].itemId;

                            playerInput.ValueRW.ammoSelectedIndex = newAmmoIndex;
                            playerInputSync.ValueRW.ammoSelectedIndex = newAmmoIndex;
                        }
                    }
                }

                if(ammoID != playerInputSync.ValueRO.ammoSelectedItemID)
                {
                    playerInputSync.ValueRW.ammoSelectedItemID = ammoID;
                    if(ammoID >= 0)
                    {
                        UIManager.instance.UpdateSelectedAmmo(newAmmoIndex);
                        if(cooldown.ValueRO.cooldownTick.IsValid  && tick.IsNewerThan(cooldown.ValueRO.cooldownTick))
                            cooldown.ValueRW.startCooldown =  EntityHelper.AddTime(tick,1);  
                        cooldown.ValueRW.cooldownTick  = EntityHelper.AddTime(tick,15);    
                    }             
                } 
            } 
            

            if (left && (!cooldown.ValueRO.cooldownTick.IsValid || tick.IsNewerThan(cooldown.ValueRO.cooldownTick)))
            {
                playerInput.ValueRW.leftButton.Set();
                playerInputSync.ValueRW.leftButton.Set();
            }
            else
            {
                playerInput.ValueRW.leftButton = default;
                playerInputSync.ValueRW.leftButton = default;
            }

            if (right && (!cooldown.ValueRO.cooldownTick.IsValid || tick.IsNewerThan(cooldown.ValueRO.cooldownTick)))
            {
                playerInput.ValueRW.rightButton.Set();
                playerInputSync.ValueRW.rightButton.Set();
            }
            else
            {
                playerInput.ValueRW.rightButton = default;
                playerInputSync.ValueRW.rightButton = default;
            }
        
            if(reloadButton)
                playerInput.ValueRW.reloadButton.Set();
            else
                playerInput.ValueRW.reloadButton = default;

            if(unloadButton)
                playerInput.ValueRW.unloadButton.Set();
            else
                playerInput.ValueRW.unloadButton = default;

        } 
        ecb.Playback(state.EntityManager);
        ecb.Dispose();  
    }

    public static void UpdateItemInHand(BufferLookup<InventorySlot> slotsLookup,BufferLookup<PlayerContainers> containersLookup,Entity player,int newSlot)
    {
        EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,new SlotPosition(EquipmentConfig.hotBar_ContainerIndex,newSlot), out var inventorySlot, out int bufferIndex);
        onNewSlotInHand?.Invoke(newSlot,inventorySlot);
    }

}
