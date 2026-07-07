using Game.Client.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;


public static class BuildingObjectCreator
{


    public static Entity CreateObjectServer(ref EntityCommandBuffer.ParallelWriter entityCommand, BuildingObjects buildingObject, int unfilteredChunkIndex)
    {
        float shadow = -0.01f * ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id).shadowPixels;
        float2 worldPos = new float2(buildingObject.globalTilePos.x * ClientMap.cellSize, buildingObject.globalTilePos.y * ClientMap.cellSize) + new float2(ClientMap.cellSize * 0.5f,0);
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox; 
        Entity entity  = entityCommand.CreateEntity(unfilteredChunkIndex);
        entityCommand.AddComponent(unfilteredChunkIndex,entity, localTransform);
        entityCommand.AddComponent(unfilteredChunkIndex,entity, new EnvironmentObject());

        if (rectangleHitbox != null)
        {
            float2 offset = rectangleHitbox.offset +  new Vector2(0,shadow);
            var boxGeometry = new BoxGeometry
            {
                Center = new float3(offset,0),
                Size = new float3(rectangleHitbox.size.x,rectangleHitbox.size.y,300f),
                Orientation = quaternion.identity,
                BevelRadius = 0f
            };
            var collisionFilter = new CollisionFilter()
            {
                BelongsTo =  1u << 10,
                CollidesWith =  (1u << 11) | (1u << 9)
            };
            var material = new Unity.Physics.Material()
            {
                Friction = 0f,
                Restitution = 0f,    
            };

            var collider = Unity.Physics.BoxCollider.Create(boxGeometry,collisionFilter,material);
            entityCommand.AddComponent(unfilteredChunkIndex,entity, new PhysicsCollider
            {
                Value = collider,
            });
            entityCommand.AddSharedComponent<PhysicsWorldIndex>(unfilteredChunkIndex,entity,new PhysicsWorldIndex());
        }
        return entity;
    }


    public static Entity CreateObject(EntitiesReferences entitiesReferences,EntityManager entityManagern,EntityCommandBuffer entityCommand, BuildingObjects buildingObject, out Entity spriteEntity)
    {
        float shadow = -0.01f * ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id).shadowPixels;
        float2 worldPos = new float2(buildingObject.globalTilePos.x * ClientMap.cellSize, buildingObject.globalTilePos.y * ClientMap.cellSize) + new float2(ClientMap.cellSize * 0.5f,0);
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox;
 
        Entity entity = entityManagern.Instantiate(entitiesReferences.buildObjectEntity);
        spriteEntity = entityManagern.GetBuffer<LinkedEntityGroup>(entity)[1].Value;

        SpriteRenderer spriteRenderer = entityManagern.GetComponentObject<SpriteRenderer>(spriteEntity);
        LocalTransform spriteTransform = LocalTransform.FromPosition(new float3(0,shadow,0));
        spriteRenderer.sprite = ItemsAsset.instance.GetBuildingObjectSprite(buildingObject.id, buildingObject.variantIndex);
        entityCommand.SetComponent(spriteEntity, spriteTransform);
        entityCommand.SetComponent(entity, localTransform);


        if (rectangleHitbox != null)
        {
            float2 offset = rectangleHitbox.offset +  new Vector2(0,shadow);
            var boxGeometry = new BoxGeometry
            {
                Center = new float3(offset,0),
                Size = new float3(rectangleHitbox.size.x,rectangleHitbox.size.y,300f),
                Orientation = quaternion.identity,
                BevelRadius = 0f
            };
            var collisionFilter = new CollisionFilter()
            {
                BelongsTo =  1u << 10,
                CollidesWith =  (1u << 11) | (1u << 9)
            };

            var material = new Unity.Physics.Material()
            {
                Friction = 0f,
                Restitution = 0f
            };

            var collider = Unity.Physics.BoxCollider.Create(boxGeometry,collisionFilter,material);
            entityCommand.SetComponent(entity, new PhysicsCollider
            {
                Value = collider
            });
            entityCommand.AddSharedComponent<PhysicsWorldIndex>(entity,new PhysicsWorldIndex());
        }
        return entity;
    }
    
}

