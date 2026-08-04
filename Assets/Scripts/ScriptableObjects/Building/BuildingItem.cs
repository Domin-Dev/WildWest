using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.Rendering.DebugUI;

[System.Serializable]
public class Drop
{
    public int maxNumber;
    [Range(0.0f, 1.0f)]
    public float probability;
    public Ingredient ingredient;
}


public abstract class BuildingItem : Item
{
    public int destructionSound;
    public AnimationEventTab HitEvents;
    public AnimationEventTab IncorrectToolHitEvent;
    public int durability;
    [Header("Drop")]
    public Drop[] drop;
    //public string texturePath;
    public ToolType toolRequired = ToolType.None;

    public virtual string GetBuildingObjectInfo(BuildingObjects buildingObjects)
    {
        return $"{name} [{buildingObjects.hitPoints}/{buildingObjects.maxHitPoints}]";
    }

    public override void SetUp()
    {
        base.SetUp();
    }
}
public abstract class VariantItem : BuildingItem
{
    public Texture2D texture;
    public ObjectVariant[] objectVariants;
    public int2 size = new int2(27, 51);
    public Vector2 shadowSize = new Vector2(0.21f,0.07f);
    public Vector2 shadowOffset;

    [Min(1)] public int damageStates = 3;
    public bool isBackground = false;
    public bool isShadow = true;
    public float shadowHeight;

    public override void SetUp()
    {
        foreach(var i in objectVariants)
            i.SetUp();
    
        base.SetUp();
    }

    public BuildingObjects GetBuildingObject(short variantIndex, short stateIndex)
    {
        return new BuildingObjects
        {
            id = ID,
            variantIndex = variantIndex,
            stateIndex = stateIndex,
            maxHitPoints = durability,
            hitPoints = durability               
        };
    }

    public Variant GetVariant(short variantIndex, short stateIndex)
    {
        return objectVariants[variantIndex].variants[stateIndex];
    }
    public Variant GetVariant(BuildingObjects obj)
    {
        return GetVariant(obj.variantIndex,obj.stateIndex);
    }
 
}

[System.Serializable]
public class ObjectVariant
{
    public Variant[] variants;
    public ObjectVariant(Variant[] variants)
    { 
        this.variants = variants;
    }
    public ObjectVariant Clone()
    {
        return new ObjectVariant(variants);
    }

    public void SetUp()
    {
        foreach(var i in variants)
            i.SetUp();
    }
}

[System.Serializable]
public class Variant
{
    public RectangleHitbox hitbox;
    public Vector2[] CoveringPoints;
    public Vector2[] objectPoints;
    public Vector2[] particlePoints;
    public Vector2 shadowOffset;
    public Vector2 offset;
    public Vector2 shadowSize = new Vector2(0.21f,0.07f);
    public Sprite[] sprites;
    private (Rect rect,float2 pivot)[] spritesData;


    public void SetUp()
    {
        Debug.Log("set up!!");
        spritesData = new  (Rect rect,float2 pivot)[sprites.Length];
        for(int i = 0; i < sprites.Length;i++)
        {
            var sprite = sprites[i];
            spritesData[i] = (sprite.rect,sprite.pivot);
        }
    }

    public Variant(Vector2 shadowSize,RectangleHitbox hitbox, Vector2 offset, Vector2[] particlePoints,Vector2? shadowOffset,params Sprite[] sprites)
    {
        this.particlePoints = particlePoints;
        this.hitbox = hitbox;
        this.sprites = sprites;
        this.offset = offset;
        this.shadowOffset = shadowOffset.HasValue ? shadowOffset.Value : default;
        this.shadowSize = shadowSize;
    }

    public void GetSpriteShape(out float2 offset,out float2 size,float margin = 0,int spriteIndex = 0)
    {
        if(spriteIndex >= sprites.Length)
        {
            offset = float2.zero;
            size = float2.zero;
            return;
        }
        
        var data = spritesData[spriteIndex];
        float convertToUnit = 1f / 100f;
        size = ((float2)data.rect.size + new float2(margin,margin) * 2f) * convertToUnit;
        offset = ((float2)data.rect.size * 0.5f - (float2)data.pivot) * convertToUnit;
    }

    public void GetHitboxShape(out float2 offset,out float2 size,float margin = 0)
    {
        offset = hitbox.offset;
        size = (float2)hitbox.size + new float2(margin,margin) * 2;
    }
}



[System.Serializable]
public class RectangleHitbox
{
    public Vector2 size;
    public Vector2 offset;
    public RectangleHitbox(Vector2 size, Vector2 offset)
    {
        this.size = size;
        this.offset = offset;
    }

    public float GetMinY()
    {
        return offset.y - size.y * 0.5f;
    }

    public static RectangleHitbox DefaultRectangleHitbox = new RectangleHitbox(new Vector2(0.1f,0.1f), Vector2.zero);
}


