
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;


public class CharacterManager : MonoBehaviour
{
    EntityManager entityManager;
    Entity player;

    private void Start()
    {
        entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
    }

    public void SetUp(Entity player)
    {
        this.player = player;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)) 
        {
            ChangeItemInHand(41, player);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ChangeItemInHand(0, player);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            ChangeItemInHand(36, player);
        }
    }

    private void ChangeItemInHand(int itemID, Entity entity)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        if (item is Weapon) SetWeaponInHand(item as Weapon, entity);
        else SetItemInHand(item, entity);
    }

    private void SetWeaponInHand(Weapon weapon, Entity entity)
    {
        Hands hands = entityManager.GetComponentData<Hands>(entity);

        SpriteRenderer spriteRenderer = entityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand);
        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(hands.itemInHand);
        LocalTransform sideHandTransform = entityManager.GetComponentData<LocalTransform>(hands.sidehand);

        if (weapon is RangedWeapon)
        {
            SetRangedWeaponInHand(weapon as RangedWeapon, ref localTransform);
        }
        else
        {
            localTransform.Position.x = -weapon.gripPoint1.x;// - 0.01f;
            localTransform.Position.y = -weapon.gripPoint1.y;
        }
        spriteRenderer.sprite = weapon.weaponImage;

        if (weapon.gripPoint2.x != -100)
        {
            Entity entity1 = entityManager.GetComponentData<Parent>(hands.itemInHand).Value;
             entityManager.SetComponentData(hands.sidehand, new Parent { Value = entity1 });
             sideHandTransform.Position = new float3(weapon.gripPoint2.x - weapon.gripPoint1.x, weapon.gripPoint2.y - weapon.gripPoint1.y, 0);
        }
        else
        {
            ResetSideHand(hands,ref sideHandTransform);
        }


        entityManager.SetComponentData(hands.itemInHand, localTransform);
        entityManager.SetComponentData(hands.sidehand, sideHandTransform);
    }

    private void SetItemInHand(Item item, Entity entity)
    {
        Hands hands = entityManager.GetComponentData<Hands>(entity);
        SpriteRenderer spriteRenderer = entityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand);
        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(hands.itemInHand);
        LocalTransform sideHandTransform = entityManager.GetComponentData<LocalTransform>(hands.sidehand);


        ResetSideHand(hands,ref sideHandTransform);
        localTransform.Position.x = 0;
        localTransform.Position.y = -0.1f;
        spriteRenderer.sprite = item.icon;
        entityManager.SetComponentData(hands.itemInHand, localTransform);
        entityManager.SetComponentData(hands.sidehand, sideHandTransform);
    }

    private void ResetSideHand(Hands hands,ref LocalTransform sideHandTransform)
    {
        entityManager.SetComponentData(hands.sidehand, new Parent { Value = hands.side });
        sideHandTransform.Position = new float3(-0.09f,0,0);
    }

    private void SetRangedWeaponInHand(RangedWeapon rangedWeapon, ref LocalTransform localTransform)
    {
        localTransform.Position.y = rangedWeapon.aimPoint.y - rangedWeapon.gripPoint1.y;
        localTransform.Position.x = -rangedWeapon.gripPoint1.x;
    }

}