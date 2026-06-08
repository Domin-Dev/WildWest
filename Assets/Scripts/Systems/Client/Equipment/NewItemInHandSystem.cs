using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.XR;


[UpdateInGroup(typeof(EquipmentSystemGroup), OrderLast = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]


partial struct NewItemInHandSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<EntityContainers> containersLookup;
    private BufferLookup<LinkedContainers> linkedContainersLookup;



    private ComponentLookup<LocalTransform> transformLookup;
    private ComponentLookup<AnimationComponent> animationLookup;
    private BufferLookup<AnimationFrames> framesLookup;
    private BufferLookup<AnimationEvents> eventsLookup;



    public static event Action<InventorySlot? ,InventorySlot[],int,InventorySlot[]> onNewItemInHand;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewItemInHandRPC>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>(true);
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<EntityContainers>(true);    
        linkedContainersLookup = SystemAPI.GetBufferLookup<LinkedContainers>(true);


        transformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
        animationLookup = SystemAPI.GetComponentLookup<AnimationComponent>();
        framesLookup = SystemAPI.GetBufferLookup<AnimationFrames>();
        eventsLookup = SystemAPI.GetBufferLookup<AnimationEvents>();
    }

    public void OnUpdate(ref SystemState state)
    {
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);
        linkedContainersLookup.Update(ref state);

        transformLookup.Update(ref state);
        animationLookup.Update(ref state);
        framesLookup.Update(ref state);
        eventsLookup.Update(ref state);

        
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();


        foreach ((RefRO<NewItemInHandRPC> rpcCommand, Entity entity) in SystemAPI.Query<RefRO<NewItemInHandRPC>>().WithEntityAccess())
        {     
            bool found = false;
            var tick = rpcCommand.ValueRO.tick;
            foreach ((RefRO<GhostOwner> owner, RefRW<Hands> hands, RefRW<PlayerInputSync> input,Entity player) in SystemAPI.Query<RefRO<GhostOwner>,RefRW<Hands>,RefRW<PlayerInputSync>>().WithAll<Player,Simulate,ContainersLoaded>().WithNone<NewPlayerTag>().WithEntityAccess())
            {
                if(rpcCommand.ValueRO.networkID == owner.ValueRO.NetworkId)
                {
                    if(SystemAPI.HasComponent<ReceiveRpcCommandRequest>(entity))
                    {
                        if(networkTime.InterpolationTick.IsNewerThan(rpcCommand.ValueRO.tick))
                        {
                            if(EQHelper.TryGetPlayerContainer(containersLookup,player,EquipmentConfig.itemInHand_ContainerIndex,out var playerContainer) && SystemAPI.Exists(playerContainer.Value.entity))
                            {    
                                EQHelper.TryGetBufferIndex(slotsLookup,0,playerContainer.Value.entity,out InventorySlot? slot, out int bufferIndex);
                                int itemId = slot.HasValue ? slot.Value.itemId : -1;

                                if(itemId == rpcCommand.ValueRO.itemID)
                                {
                                    Item item = ItemsAsset.instance.GetItem(itemId);
                                    ChangeItemInHand(ref state,item, hands);

                                    if(state.EntityManager.IsComponentEnabled<GhostOwnerIsLocal>(player))
                                    {
                                        CharacterHandsEvents.ResetAnimation(hands.ValueRO,animationLookup,transformLookup,framesLookup,eventsLookup);
                                        state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(rpcCommand.ValueRO.tick,20)});
                                        if(EQHelper.TryGetPlayerContainer(containersLookup,player,EquipmentConfig.hotBar_ContainerIndex,out var hotbar))
                                        {
                                            UpdateUI(ref state,input,player,slot,out var ammoID,hotbar.Value.entity);
                                        }
                                    }

                                    entityCommandBuffer.DestroyEntity(entity);
                                }
                            }
                        }
                        else
                        {
                            var request = SystemAPI.GetComponent<ReceiveRpcCommandRequest>(entity);
                            request.Consume();
                            entityCommandBuffer.SetComponent(entity,request);
                        }
                    }
                    else
                    {
                        if(EQHelper.TryGetPlayerContainer(containersLookup,player,EquipmentConfig.hotBar_ContainerIndex,out var playerContainer))
                        {
                            Debug.Log("container "+ playerContainer.Value.entity + " " + playerContainer.Value.index);
                            EQHelper.TryGetBufferIndex(slotsLookup,input.ValueRO.slotInHand,playerContainer.Value.entity,out InventorySlot? slot , out int bufferIndex);
                            int itemId = slot.HasValue ? slot.Value.itemId : -1;
                            Item item = ItemsAsset.instance.GetItem(itemId);
                            ChangeItemInHand(ref state,item,hands);
                            
                            if(state.EntityManager.IsComponentEnabled<GhostOwnerIsLocal>(player))
                            {
                                var cooldownTick = tick;
                                if(tick == NetworkTick.Invalid)
                                    cooldownTick = networkTime.ServerTick;

                                CharacterHandsEvents.ResetAnimation(hands.ValueRO,animationLookup,transformLookup,framesLookup,eventsLookup);
                                //Debug.Log("new item!!! " + cooldownTick.TickIndexForValidTick + "  " + EntityHelper.AddTime(cooldownTick,20).TickIndexForValidTick);
                                state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(cooldownTick,20)});
                                UpdateUI(ref state,input,player,slot,out var ammoID,playerContainer.Value.entity);
                            }
                            entityCommandBuffer.DestroyEntity(entity);

                           // if(ItemsAsset.instance.TryGetItem<RangedWeapon>(itemId,out var item) && ammoID >= 0)
                            //{
                                // EntityHelper.CreateEntityWithComponent(entityCommandBuffer,new NewAmmoSelectedRPC()
                                // {
                                //     ammoID = ammoID,
                                //     weaponID = itemId,
                                //     networkID = owner.ValueRO.NetworkId 
                                // }); 
                           // }
                         //   if(ItemsAsset.instance.TryGetItem<RangedWeapon>(itemId,out var item) && animArgs != null)
                            //    CharacterHandsEvents.StartAnimation(ref state,item,hands.ValueRO,item.reloadAnim,animationLookup,transformLookup,framesLookup,eventsLookup,animArgs); 
                        }
                    }
                    

                    
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                Debug.Log("interpolation tick " + networkTime.InterpolationTick.IsValid + " " + tick.IsValid);
                if(!tick.IsValid || networkTime.InterpolationTick.IsNewerThan(EntityHelper.AddTime(tick,200)))
                {
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }

        }   
    }

    public static void UpdateItemInHandUI(InventorySlot? item ,InventorySlot[] ammo,int selectedAmmoIndex, InventorySlot[] magazine)
    {
        onNewItemInHand?.Invoke(item,ammo,selectedAmmoIndex,magazine);    
    }

    private void UpdateUI(ref SystemState state,RefRW<PlayerInputSync> input, Entity player, InventorySlot? itemSlot,out int ammoID, Entity hotBarContainer)
    {
        ammoID = -1;      
        InventorySlot[] ammo = null;
        InventorySlot[] magazine = null;

        int selectedAmmo = -1;

        if(itemSlot.HasValue && ItemsAsset.instance.TryGetItem<RangedWeapon>(itemSlot.Value.itemId,out var item))
        {
            ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
            if(ammo.Length > 0)
            {
                selectedAmmo = state.EntityManager.GetComponentData<PlayerInput>(player).ammoSelectedIndex % ammo.Length;
                ammoID = ammo[selectedAmmo].itemId;
            }
            NewEquipmentManager.instance.SetAmmoTag(item.ammoTagID); 
            input.ValueRW.ammoSelectedItemID = ammoID;

            Debug.Log("item!!!!!!!!!! + "+ itemSlot.Value.itemId);
            if(item.hasMagazine)
            {
                magazine = EQHelper.ReadLinkedContainer(slotsLookup,linkedContainersLookup,hotBarContainer,input.ValueRO.slotInHand);
            }
        }
        else
        {
            NewEquipmentManager.instance.SetAmmoTag(-1);
            input.ValueRW.ammoSelectedItemID = -1;
        }
        
        onNewItemInHand?.Invoke(itemSlot,ammo,selectedAmmo,magazine);    
    }    

    public static void ChangeItemInHand(ref SystemState state,Item item, RefRW<Hands> hands)
    {
        Crosshairs.Swtich(item is RangedWeapon);


        if (item is Weapon)
        {
            SetWeaponInHand(hands,item as Weapon,ref state);
        }
        else 
        {
            SetItemInHand(hands,item, ref state);
        }
    }
    private static void SetWeaponInHand(RefRW<Hands> hands,Weapon weapon,ref SystemState state)
    {
        SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(hands.ValueRO.itemInMainHand);
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInMainHand);
        
        LocalTransform aimPoint = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.aimPoint);
        LocalTransform reloadPoint = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.reloadPoint);
        LocalTransform sideHandTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.sidehand);
        LocalTransform mainHand = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
        mainHand.Position.x = weapon.handOffset;


        if (weapon is RangedWeapon)
        {
            SetRangedWeaponInHand(weapon as RangedWeapon,ref mainHand, ref localTransform, ref aimPoint,ref reloadPoint);
        }
        else
        {
            localTransform.Position.x = -weapon.gripPoint1.x;// - 0.01f;
            localTransform.Position.y = -weapon.gripPoint1.y;

            mainHand.Position.y = 0;
        }

        spriteRenderer.sprite = weapon.weaponImage;

        if (weapon.gripPoint2.x != -100)
        {
            Entity entity1 = state.EntityManager.GetComponentData<Parent>(hands.ValueRO.itemInMainHand).Value;
            state.EntityManager.SetComponentData(hands.ValueRO.sidehand, new Parent { Value = entity1 });
            sideHandTransform.Position = new float3(weapon.gripPoint2.x - weapon.gripPoint1.x, weapon.gripPoint2.y - weapon.gripPoint1.y, 0);
            hands.ValueRW.twoHanded = true;
        }
        else
        {
            ResetSideHand(hands,ref sideHandTransform, ref state);
        }

        state.EntityManager.GetComponentObject<SpriteRenderer>(hands.ValueRO.itemInSideHand).sprite = null;
        state.EntityManager.SetComponentData(hands.ValueRO.itemInMainHand, localTransform);
        state.EntityManager.SetComponentData(hands.ValueRO.sidehand, sideHandTransform);
        state.EntityManager.SetComponentData(hands.ValueRO.aimPoint, aimPoint);
        state.EntityManager.SetComponentData(hands.ValueRO.reloadPoint, reloadPoint);
        state.EntityManager.SetComponentData(hands.ValueRO.mainhand, mainHand);
    }
    private static void SetItemInHand(RefRW<Hands> hands,Item item, ref SystemState state)
    {
        SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(hands.ValueRO.itemInMainHand);
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.itemInMainHand);
        LocalTransform sideHandTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.sidehand);

        LocalTransform mainHand = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);

        mainHand.Position.y = 0;
        mainHand.Position.x = 0.07f;



        ResetSideHand(hands,ref sideHandTransform,ref state);
        localTransform.Position.x = 0;
        localTransform.Position.y = 0;
        spriteRenderer.sprite = null;

        if (item != null)
            spriteRenderer.sprite = item.icon;

        state.EntityManager.GetComponentObject<SpriteRenderer>(hands.ValueRO.itemInSideHand).sprite = null;
        state.EntityManager.SetComponentData(hands.ValueRO.itemInMainHand, localTransform);
        state.EntityManager.SetComponentData(hands.ValueRO.sidehand, sideHandTransform);
        state.EntityManager.SetComponentData(hands.ValueRO.mainhand, mainHand);
    }
    private static void ResetSideHand(RefRW<Hands>  hands,ref LocalTransform sideHandTransform, ref SystemState state)
    {
        state.EntityManager.SetComponentData(hands.ValueRO.sidehand, new Parent { Value = hands.ValueRO.side });
        sideHandTransform.Position = new float3(-0.09f,0,0);
        hands.ValueRW.twoHanded = false;
    }
    private static void SetRangedWeaponInHand(RangedWeapon rangedWeapon,ref LocalTransform mainHand, ref LocalTransform localTransform, ref LocalTransform aimPoint, ref LocalTransform reloadPoint)
    {
        mainHand.Position.y = -rangedWeapon.aimPoint.y + rangedWeapon.gripPoint1.y;

        localTransform.Position.y = -rangedWeapon.gripPoint1.y;
        localTransform.Position.x = -rangedWeapon.gripPoint1.x;

        aimPoint.Position.x = rangedWeapon.aimPoint.x +  localTransform.Position.x;
        aimPoint.Position.y = rangedWeapon.aimPoint.y +  localTransform.Position.y;

        reloadPoint.Position.x = rangedWeapon.reloadPoint.x + localTransform.Position.x;
        reloadPoint.Position.y = rangedWeapon.reloadPoint.y + localTransform.Position.y;
    }
}
