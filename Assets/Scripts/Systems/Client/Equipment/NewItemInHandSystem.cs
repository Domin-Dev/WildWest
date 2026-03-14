using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;


[UpdateInGroup(typeof(EquipmentSystemGroup), OrderLast = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]


partial struct NewItemInHandSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> containersLookup;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewItemInHandRPC>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>(true);
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);
        
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var snapshotAck = SystemAPI.GetSingleton<NetworkSnapshotAck>();


        foreach ((RefRO<NewItemInHandRPC> rpcCommand, Entity entity) in SystemAPI.Query<RefRO<NewItemInHandRPC>>().WithEntityAccess())
        {     
            bool found = false;
            var tick = rpcCommand.ValueRO.tick;
            Debug.Log("nowy!!!");

            foreach ((RefRO<GhostOwner> owner, RefRO<Hands> hands, RefRO<PlayerInputSync> input,Entity e) in SystemAPI.Query<RefRO<GhostOwner>,RefRO<Hands>,RefRO<PlayerInputSync>>().WithAll<Player,Simulate>().WithNone<NewPlayerTag>().WithEntityAccess())
            {
                if(rpcCommand.ValueRO.networkID == owner.ValueRO.NetworkId)
                {
                    if(SystemAPI.HasComponent<ReceiveRpcCommandRequest>(entity))
                    {
                        if(EQHelper.TryGetPlayerContainer(containersLookup,e,EquipmentConfig.itemInHand_ContainerIndex,out var playerContainer))
                        {
                            Debug.Log("uwaga!!" + snapshotAck.LastReceivedSnapshotByLocal.TickIndexForValidTick + "  " + rpcCommand.ValueRO.tick.TickIndexForValidTick);
                            if(snapshotAck.LastReceivedSnapshotByLocal.IsNewerThan(rpcCommand.ValueRO.tick))
                            {
                                Debug.Log("remoce!!");
                                EQHelper.TryGetBufferIndex(slotsLookup,0,playerContainer.Value.entity,out int itemId, out int bufferIndex);
                                ChangeItemInHand(ref state,itemId,entityCommandBuffer,in hands.ValueRO);
                                entityCommandBuffer.DestroyEntity(entity);
                            }
                        }
                        SystemAPI.GetComponent<ReceiveRpcCommandRequest>(entity).Consume();
                    }
                    else
                    {
                        if(EQHelper.TryGetPlayerContainer(containersLookup,e,EquipmentConfig.hotBar_ContainerIndex,out var playerContainer))
                        {
                            EQHelper.TryGetBufferIndex(slotsLookup,input.ValueRO.slotInHand,playerContainer.Value.entity,out int itemId, out int bufferIndex);
                            ChangeItemInHand(ref state,itemId,entityCommandBuffer,in hands.ValueRO);
                            entityCommandBuffer.DestroyEntity(entity);
                        }
                    }
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                tick.Add(50u);
                if(snapshotAck.LastReceivedSnapshotByLocal.IsNewerThan(tick))
                {
                    Debug.Log("nie znaleziono");
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }

        }   
    }

    

    public void ChangeItemInHand(ref SystemState state,int itemID, EntityCommandBuffer ecb,in Hands hands)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);

        if (item is Weapon) SetWeaponInHand(ecb,hands,item as Weapon,ref state);
        else SetItemInHand(ecb,hands,item, ref state);
    }
    private void SetWeaponInHand(EntityCommandBuffer entityCommandBuffer,Hands hands,Weapon weapon,ref SystemState state)
    {
        SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand);
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.itemInHand);
        
        LocalTransform aimPoint = state.EntityManager.GetComponentData<LocalTransform>(hands.aimPoint);
        LocalTransform reloadPoint = state.EntityManager.GetComponentData<LocalTransform>(hands.reloadPoint);
        LocalTransform sideHandTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.sidehand);
        LocalTransform mainHand = state.EntityManager.GetComponentData<LocalTransform>(hands.mainhand);

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
            Entity entity1 = state.EntityManager.GetComponentData<Parent>(hands.itemInHand).Value;
            entityCommandBuffer.SetComponent(hands.sidehand, new Parent { Value = entity1 });
            sideHandTransform.Position = new float3(weapon.gripPoint2.x - weapon.gripPoint1.x, weapon.gripPoint2.y - weapon.gripPoint1.y, 0);
        }
        else
        {
            ResetSideHand(entityCommandBuffer,hands,ref sideHandTransform, ref state);
        }


        entityCommandBuffer.SetComponent(hands.itemInHand, localTransform);
        entityCommandBuffer.SetComponent(hands.sidehand, sideHandTransform);
        entityCommandBuffer.SetComponent(hands.aimPoint, aimPoint);
        entityCommandBuffer.SetComponent(hands.reloadPoint, reloadPoint);
        entityCommandBuffer.SetComponent(hands.mainhand, mainHand);
    }
    private void SetItemInHand(EntityCommandBuffer entityCommandBuffer,Hands hands,Item item, ref SystemState state)
    {
        SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand);
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.itemInHand);
        LocalTransform sideHandTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.sidehand);

        LocalTransform mainHand = state.EntityManager.GetComponentData<LocalTransform>(hands.mainhand);

        mainHand.Position.y = 0;

        ResetSideHand(entityCommandBuffer,hands,ref sideHandTransform,ref state);
        localTransform.Position.x = 0;
        localTransform.Position.y = 0;
        spriteRenderer.sprite = null;

        if (item != null)
            spriteRenderer.sprite = item.icon;
        entityCommandBuffer.SetComponent(hands.itemInHand, localTransform);
        entityCommandBuffer.SetComponent(hands.sidehand, sideHandTransform);
        entityCommandBuffer.SetComponent(hands.mainhand, mainHand);
    }
    private void ResetSideHand(EntityCommandBuffer ecb,Hands hands,ref LocalTransform sideHandTransform, ref SystemState state)
    {
         ecb.SetComponent(hands.sidehand, new Parent { Value = hands.side });
        sideHandTransform.Position = new float3(-0.09f,0,0);
    }
    private void SetRangedWeaponInHand(RangedWeapon rangedWeapon,ref LocalTransform mainHand, ref LocalTransform localTransform, ref LocalTransform aimPoint, ref LocalTransform reloadPoint)
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
