
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CharacterManager : MonoBehaviour
{
    Entity player;


    public static CharacterManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        else
            Destroy(this);

        EquipmentManager.instance.UpdateItemInHand += UpdateItemInHand;
    }


    private void OnDestroy()
    {
        EquipmentManager.instance.UpdateItemInHand -= UpdateItemInHand;
    }



    private void UpdateItemInHand(object sender, ItemStatsArgs e)
    {
        int id = -1;
        if (e.item != null)
        {
            id = e.item.itemID;
        }


        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PlayerInput), typeof(GhostOwnerIsLocal), typeof(Simulate));
        var entities = entityQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
        if (entities.Length > 0)
        {
            Entity entity = entities[0];

            ItemInHandInput itemInHandInput = entityManager.GetComponentData<ItemInHandInput>(entity);
            if (itemInHandInput.itemInHand != id)
            {
                ItemInHandInputSync itemInHandInputSync = entityManager.GetComponentData<ItemInHandInputSync>(entity);

                itemInHandInput.itemInHand = id;
                itemInHandInputSync.itemInHand = id;

                entityManager.SetComponentData(entity, itemInHandInputSync);
                entityManager.SetComponentData(entity, itemInHandInput);
            }
        }
        entityQuery.Dispose();
        entities.Dispose();
    }

    public void ChangeItemInHand(int itemID, Entity entity, ref SystemState state)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        if (item is Weapon) SetWeaponInHand(item as Weapon, entity,ref state);
        else SetItemInHand(item, entity, ref state);
    }
    private void SetWeaponInHand(Weapon weapon, Entity entity, ref SystemState state)
    {
        Hands hands = state.EntityManager.GetComponentData<Hands>(entity);

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
            state.EntityManager.SetComponentData(hands.sidehand, new Parent { Value = entity1 });
            sideHandTransform.Position = new float3(weapon.gripPoint2.x - weapon.gripPoint1.x, weapon.gripPoint2.y - weapon.gripPoint1.y, 0);
        }
        else
        {
            ResetSideHand(hands,ref sideHandTransform, ref state);
        }


        state.EntityManager.SetComponentData(hands.itemInHand, localTransform);
        state.EntityManager.SetComponentData(hands.sidehand, sideHandTransform);
        state.EntityManager.SetComponentData(hands.aimPoint, aimPoint);
        state.EntityManager.SetComponentData(hands.reloadPoint, reloadPoint);
        state.EntityManager.SetComponentData(hands.mainhand, mainHand);
    }
    private void SetItemInHand(Item item, Entity entity, ref SystemState state)
    {
        Hands hands = state.EntityManager.GetComponentData<Hands>(entity);
        SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand);
        LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.itemInHand);
        LocalTransform sideHandTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.sidehand);

        LocalTransform mainHand = state.EntityManager.GetComponentData<LocalTransform>(hands.mainhand);

        mainHand.Position.y = 0;

        ResetSideHand(hands,ref sideHandTransform,ref state);
        localTransform.Position.x = 0;
        localTransform.Position.y = -0.1f;
        spriteRenderer.sprite = null;
        if (item != null)
            spriteRenderer.sprite = item.icon;
        state.EntityManager.SetComponentData(hands.itemInHand, localTransform);
        state.EntityManager.SetComponentData(hands.sidehand, sideHandTransform);
        state.EntityManager.SetComponentData(hands.mainhand, mainHand);
    }
    private void ResetSideHand(Hands hands,ref LocalTransform sideHandTransform, ref SystemState state)
    {
        state.EntityManager.SetComponentData(hands.sidehand, new Parent { Value = hands.side });
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