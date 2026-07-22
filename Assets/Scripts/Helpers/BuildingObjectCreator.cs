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
        var data  = ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id).objectVariants[buildingObject.variantIndex].variants[buildingObject.stateIndex];
    
        float2 worldPos = new float2(buildingObject.globalTilePos.x * ClientMap.cellSize, buildingObject.globalTilePos.y * ClientMap.cellSize) + new float2(ClientMap.cellSize/2f,data.minY);
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox; 
        Entity entity  = entityCommand.CreateEntity(unfilteredChunkIndex);
        entityCommand.AddComponent(unfilteredChunkIndex,entity, localTransform);
        entityCommand.AddComponent(unfilteredChunkIndex,entity, new EnvironmentObject());

        if (rectangleHitbox != null && rectangleHitbox.offset.sqrMagnitude != 0)
        {
            float2 offset = rectangleHitbox.offset;
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
        var item  = ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id);
        var data = item.objectVariants[buildingObject.variantIndex].variants[buildingObject.stateIndex];
        
        float2 worldPos = new float2(buildingObject.globalTilePos.x * ClientMap.cellSize, buildingObject.globalTilePos.y * ClientMap.cellSize) + new float2( ClientMap.cellSize/2f,data.minY);
       
       
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, item.isBackground ? worldPos.y + 0.25f : worldPos.y ));
        RectangleHitbox rectangleHitbox = ItemsAsset.instance.GetVariant(buildingObject.id, buildingObject.variantIndex, buildingObject.stateIndex)?.hitbox;
 
        Entity entity = entityManagern.Instantiate(entitiesReferences.buildObjectEntity);
        spriteEntity = entityManagern.GetBuffer<LinkedEntityGroup>(entity)[1].Value;
        Entity shadowEntity = entityManagern.GetBuffer<LinkedEntityGroup>(entity)[2].Value;

        SpriteRenderer spriteRenderer = entityManagern.GetComponentObject<SpriteRenderer>(spriteEntity);
        LocalTransform spriteTransform = LocalTransform.FromPosition(new float3(0,0,0));
        var sprite =  ItemsAsset.instance.GetBuildingObjectSprite(buildingObject.id, buildingObject.variantIndex);
        spriteRenderer.sprite = sprite;
        entityCommand.SetComponent(spriteEntity, spriteTransform);
        entityCommand.SetComponent(entity, localTransform);

        if(item.isShadow)
        {
            entityManagern.GetComponentObject<SpriteRenderer>(shadowEntity).size = data.shadowSize;
            entityCommand.SetComponent(shadowEntity,LocalTransform.FromPosition(new float3(data.shadowPoint.x,data.shadowPoint.y + 0.005f,0)));
        }
        else
            entityManagern.GetComponentObject<SpriteRenderer>(shadowEntity).sprite = null;
        

        if (rectangleHitbox != null && rectangleHitbox.offset.sqrMagnitude != 0)
        {
            float2 offset = rectangleHitbox.offset;
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

