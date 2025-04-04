
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


public class CharacterManager : MonoBehaviour
{
    EntityManager entityManager;
    Entity player;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
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
    }

    private void ChangeItemInHand(int itemID, Entity entity)
    {
        Weapon weapon = ItemsAsset.instance.GetItem<Weapon>(itemID);
        Hands hands = entityManager.GetComponentData<Hands>(entity);
        SpriteRenderer spriteRenderer = entityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand);
        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(hands.itemInHand);
        localTransform.Position.x = weapon.gripPoint1.x;
        localTransform.Position.y = -weapon.gripPoint1.y;
        spriteRenderer.sprite = weapon.weaponImage;

        entityManager.SetComponentData(hands.itemInHand, localTransform); 
    }

}