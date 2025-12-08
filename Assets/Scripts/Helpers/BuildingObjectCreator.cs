using Game.Client.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;


public static class BuildingObjectCreator
{


    public static Entity CreateObjectServer(EntitiesReferences entitiesReferences, ref EntityCommandBuffer.ParallelWriter entityCommand, BuildingObjects buildingObject, int unfilteredChunkIndex)
    {
        float shadow = -0.01f * ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id).shadowPixels;
        float2 worldPos = new float2(buildingObject.position.x * ClientMap.cellSize, buildingObject.position.y * ClientMap.cellSize) + new float2(ClientMap.cellSize * 0.5f,0);
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox;
           
        Entity entity  = entityCommand.CreateEntity(unfilteredChunkIndex);
        entityCommand.AddComponent(unfilteredChunkIndex,entity, localTransform);
        entityCommand.AddComponent(unfilteredChunkIndex, entity, new Physics2D()
        {
            layer = 0,
            cellIndex = new int2(int.MinValue, int.MinValue)
        });     
        
        
        if (rectangleHitbox != null)
        {
            entityCommand.AddComponent(unfilteredChunkIndex,entity, new IsChanged());
            entityCommand.SetComponentEnabled(unfilteredChunkIndex,entity, typeof(IsChanged), true);
            entityCommand.AddComponent(unfilteredChunkIndex,entity, new BoxCollider2D()
            {
                offset = rectangleHitbox.offset +  new Vector2(0,shadow),
                size = rectangleHitbox.size
            });
            entityCommand.AddComponent(unfilteredChunkIndex,entity, new Velocity2D() { Value = float2.zero });
        }
        return entity;
    }


    public static Entity CreateObject(EntitiesReferences entitiesReferences,EntityManager entityManagern,EntityCommandBuffer entityCommand, BuildingObjects buildingObject)
    {
        float shadow = -0.01f * ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id).shadowPixels;
        float2 worldPos = new float2(buildingObject.position.x * ClientMap.cellSize, buildingObject.position.y * ClientMap.cellSize) + new float2(ClientMap.cellSize * 0.5f,0);
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox;
 
        Entity entity = entityManagern.Instantiate(entitiesReferences.buildObjectEntity);


        Entity sprite = entityManagern.GetBuffer<LinkedEntityGroup>(entity)[1].Value;
        SpriteRenderer spriteRenderer = entityManagern.GetComponentObject<SpriteRenderer>(sprite);
        LocalTransform spriteTransform = LocalTransform.FromPosition(new float3(0,shadow,0));
        spriteRenderer.sprite = ItemsAsset.instance.GetBuildingObjectSprite(buildingObject.id, buildingObject.variantIndex);
        entityCommand.SetComponent(sprite, spriteTransform);
        entityCommand.SetComponent(entity, localTransform);


        if (rectangleHitbox != null)
        {
            entityCommand.AddComponent(entity, new IsChanged());
            entityCommand.SetComponentEnabled(entity, typeof(IsChanged), true);
            entityCommand.AddComponent(entity, new BoxCollider2D()
            {
                offset = rectangleHitbox.offset +  new Vector2(0,shadow),
                size = rectangleHitbox.size
            });
            entityCommand.AddComponent(entity, new Velocity2D() { Value = float2.zero });
        }
        return entity;
    }
    
}

