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
    public static Entity CreateObjectServer(ref EntityCommandBuffer.ParallelWriter ecb, BuildingObjects buildingObject, int unfilteredChunkIndex)
    {
        var item  = ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id);
        var variant = item.objectVariants[buildingObject.variantIndex].variants[buildingObject.stateIndex];

        float2 worldPos = new float2(buildingObject.globalTilePos.x * ClientMap.cellSize, buildingObject.globalTilePos.y * ClientMap.cellSize) + (float2)variant.offset;
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, worldPos.y));
        RectangleHitbox rectangleHitbox = variant?.hitbox; 
        Entity entity  = ecb.CreateEntity(unfilteredChunkIndex);
        ecb.AddComponent(unfilteredChunkIndex,entity, localTransform);
        ecb.AddComponent<LocalToWorld>(unfilteredChunkIndex,entity);
        ecb.AddComponent(unfilteredChunkIndex,entity, new EnvironmentObject());
        ecb.AddBuffer<LinkedEntityGroup>(unfilteredChunkIndex,entity);

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
            ecb.AddComponent(unfilteredChunkIndex,entity, new PhysicsCollider
            {
                Value = collider,
            });
            ecb.AddSharedComponent<PhysicsWorldIndex>(unfilteredChunkIndex,entity,new PhysicsWorldIndex());
        }

        CreateTriggers(true,ecb,item,variant,entity,unfilteredChunkIndex);
        return entity;
    }

    private static void CreateTriggers(bool isSerwer,EntityCommandBuffer.ParallelWriter ecb,VariantItem item,Variant variant,Entity gridObject,int unfilteredChunkIndex)
    {
        if(ItemsAsset.instance.ItemHasTheTagType<TagWithTrigger>(item,out (TagWithTrigger tag,TagSettings TagSettings)[] tags))
        {
            foreach( (TagWithTrigger tag,TagSettings tagSettings) in tags)
            {
                if((isSerwer && tag.worldType == WorldType.Client) || (!isSerwer && tag.worldType == WorldType.Server))
                    continue;

                var tagSettingsTrigger = tag.GetTagSettings(tagSettings);
                GetTriggerData(variant,tagSettingsTrigger.triggerShape,tagSettingsTrigger.margin,out float2 size,out float2 offset);

                var boxGeometry = new BoxGeometry
                {
                    Center = new float3(offset,0f),
                    Size = new float3(size,300f),
                    Orientation = quaternion.identity,
                    BevelRadius = 0f
                };
                var collisionFilter = new CollisionFilter()
                {
                    BelongsTo =  (uint)tag.BelongsTo.value,
                    CollidesWith = (uint)tag.CollidesWith.value,
                };
                var material = new Unity.Physics.Material()
                {
                    Friction = 0f,
                    Restitution = 0f,   
                    CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents,
                };
                var collider = Unity.Physics.BoxCollider.Create(boxGeometry,collisionFilter,material);

                var entity = ecb.CreateEntity(unfilteredChunkIndex);


                ecb.AddComponent(unfilteredChunkIndex,entity,LocalTransform.FromPosition(float3.zero));
                ecb.AddComponent<LocalToWorld>(unfilteredChunkIndex,entity);
                ecb.AddComponent(unfilteredChunkIndex,entity, new PhysicsCollider
                {
                    Value = collider,
                });
                ecb.AddSharedComponent<PhysicsWorldIndex>(unfilteredChunkIndex,entity,new PhysicsWorldIndex());
                ecb.AddComponent(unfilteredChunkIndex,entity,new TriggerTagComponent()
                {
                     TagID = tag.ID,
                     itemID = item.ID
                });
                ecb.AddComponent<HasEvents>(unfilteredChunkIndex,entity);
                ecb.AddBuffer<StatefulTriggerEvent>(unfilteredChunkIndex,entity);
                ecb.AddBuffer<TagActionState>(unfilteredChunkIndex,entity);
                ecb.AddComponent(unfilteredChunkIndex,entity, new Parent
                {
                    Value = gridObject
                });
                ecb.AppendToBuffer<LinkedEntityGroup>(unfilteredChunkIndex,gridObject,new LinkedEntityGroup()
                {
                    Value = entity
                });
            }
        }
    }
   

    public static Entity CreateObject(EntitiesReferences entitiesReferences,EntityManager entityManagern,EntityCommandBuffer ecb, BuildingObjects buildingObject, out Entity spriteEntity)
    {
        var item  = ItemsAsset.instance.GetItem<VariantItem>(buildingObject.id);
        var variant = item.objectVariants[buildingObject.variantIndex].variants[buildingObject.stateIndex];
        
        float2 worldPos = new float2(buildingObject.globalTilePos.x * ClientMap.cellSize, buildingObject.globalTilePos.y * ClientMap.cellSize) + (float2)variant.offset;
    
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(worldPos.x, worldPos.y, item.isBackground ? worldPos.y + 0.25f : worldPos.y ));
        RectangleHitbox rectangleHitbox = variant?.hitbox;
 
        Entity entity = entityManagern.Instantiate(entitiesReferences.buildObjectEntity);

        var gridObj = entityManagern.GetComponentData<ClientGridObject>(entity);      
        spriteEntity = gridObj.sprite;
        Entity shadowEntity = gridObj.shadow;

        SpriteRenderer spriteRenderer = entityManagern.GetComponentObject<SpriteRenderer>(spriteEntity);
        LocalTransform spriteTransform = LocalTransform.FromPosition(new float3(0,0,0));
        var sprite =  ItemsAsset.instance.GetBuildingObjectSprite(buildingObject.id, buildingObject.variantIndex);
        spriteRenderer.sprite = sprite;

        if(ItemsAsset.instance.ItemHasTheTag(item,WorldConfig.Instance.WindEffectTag,out var tagSelection))
            spriteRenderer.material = tagSelection.material;
        
        ecb.SetComponent(spriteEntity, spriteTransform);
        ecb.SetComponent(entity, localTransform);

        CreateTriggers(false,ecb,item,variant,entity);



        if(item.isShadow)
        {
            entityManagern.GetComponentObject<SpriteRenderer>(shadowEntity).size = variant.shadowSize;
            ecb.SetComponent(shadowEntity,LocalTransform.FromPosition(new float3(variant.shadowOffset,0)));
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
            ecb.AddComponent(entity, new PhysicsCollider
            {
                Value = collider
            });
            ecb.AddSharedComponent<PhysicsWorldIndex>(entity,new PhysicsWorldIndex());
        }
        return entity;
    }


    private static void CreateTriggers(bool isSerwer,EntityCommandBuffer ecb,VariantItem item,Variant variant,Entity gridObject)
    {
        if(ItemsAsset.instance.ItemHasTheTagType<TagWithTrigger>(item,out (TagWithTrigger tag,TagSettings TagSettings)[] tags))
        {
            foreach( (TagWithTrigger tag,TagSettings tagSettings) in tags)
            {
                if((isSerwer && tag.worldType == WorldType.Client) || (!isSerwer && tag.worldType == WorldType.Server))
                    continue;

                var tagSettingsTrigger = tag.GetTagSettings(tagSettings);
                GetTriggerData(variant,tagSettingsTrigger.triggerShape,tagSettingsTrigger.margin,out float2 size,out float2 offset);

                var boxGeometry = new BoxGeometry
                {
                    Center = new float3(offset,0f),
                    Size = new float3(size,300f),
                    Orientation = quaternion.identity,
                    BevelRadius = 0f
                };
                var collisionFilter = new CollisionFilter()
                {
                    BelongsTo =  (uint)tag.BelongsTo.value,
                    CollidesWith = (uint)tag.CollidesWith.value,
                };
                var material = new Unity.Physics.Material()
                {
                    Friction = 0f,
                    Restitution = 0f,   
                    CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents,
                };
                var collider = Unity.Physics.BoxCollider.Create(boxGeometry,collisionFilter,material);

                var entity = ecb.CreateEntity();
                ecb.AddComponent(entity, new Parent
                {
                    Value = gridObject
                });
                ecb.AddComponent(entity,LocalTransform.FromPosition(float3.zero));
                ecb.AddComponent<LocalToWorld>(entity);
                ecb.AddComponent(entity, new PhysicsCollider
                {
                    Value = collider,
                });
                ecb.AddSharedComponent<PhysicsWorldIndex>(entity,new PhysicsWorldIndex());
                ecb.AddComponent(entity,new TriggerTagComponent()
                {
                     TagID = tag.ID,
                     itemID = item.ID
                });
                ecb.AddComponent<HasEvents>(entity);
                ecb.AddBuffer<StatefulTriggerEvent>(entity);
                ecb.AddBuffer<TagActionState>(entity);
                ecb.AppendToBuffer<LinkedEntityGroup>(gridObject,new LinkedEntityGroup()
                {
                    Value = entity
                });
            }
        }
    }
    private static void GetTriggerData(Variant variant,TriggerShape shape,float margin,out float2 size,out float2 offset)
    {
        switch(shape)
        {
            case TriggerShape.SpriteShape : variant.GetSpriteShape(out offset,out size,margin,0);
                return;
            case TriggerShape.HitboxShape : variant.GetHitboxShape(out offset,out size,margin);
                return;
        }
        size = float2.zero;
        offset = float2.zero;
    }


}

