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
    public static Entity CreateObject(ref EntitiesReferences entitiesReferences,EntityManager entityManager, ref EntityCommandBuffer entityCommand, BuildingObjects buildingObject)
    {
  //      Debug.Log("New OBJ  " + entityManager.World.Flags);
        float shadow = -0.01f * ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id).shadowPixels;

        float2 worldPos = new float2(buildingObject.position.x * ClientMap.cellSize, buildingObject.position.y * ClientMap.cellSize) + new float2(ClientMap.cellSize * 0.5f,0);
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox;
        Entity entity;


        if (entityManager.World.IsServer())
        {
            entity  = entityManager.CreateEntity();
            entityCommand.AddComponent(entity, localTransform);
            entityCommand.AddComponent(entity, new Physics2D()
            {
                layer = 0,
                cellIndex = new int2(int.MinValue, int.MinValue)
            });
        }
        else
        {

            entity = entityManager.Instantiate(entitiesReferences.buildObjectEntity);
            Entity sprite = entityManager.GetBuffer<LinkedEntityGroup>(entity)[1].Value;
            SpriteRenderer spriteRenderer = entityManager.GetComponentObject<SpriteRenderer>(sprite);
            LocalTransform spriteTransform = LocalTransform.FromPosition(new float3(0,shadow,0));

            spriteRenderer.sprite = ItemsAsset.instance.GetBuildingObjectSprite(buildingObject.id, buildingObject.variantIndex);
            entityCommand.SetComponent(entity, localTransform);
            entityCommand.SetComponent(sprite, spriteTransform);

        }

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

