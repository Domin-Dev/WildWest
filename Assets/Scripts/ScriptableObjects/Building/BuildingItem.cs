using System.Collections.Generic;
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

    public override void OnAfterDeserialize()
    {
        base.OnAfterDeserialize();
    }
}
public abstract class VariantItem : BuildingItem
{
    public Texture2D texture;
    public ObjectVariant[] objectVariants;
    public int2 size = new int2(27, 51);
    public Vector2 shadowSize = new Vector2(0.21f,0.07f);

    [Min(1)] public int damageStates = 3;
    public bool isBackground = false;
    public bool isShadow = true;
    public float shadowHeight;

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
}

[System.Serializable]
public class Variant
{
    public RectangleHitbox hitbox;
    public Vector2[] CoveringPoints;
    public Vector2[] objectPoints;
    public Vector2[] particlePoints;
    public Vector2 shadowPoint;
    public float minY;
    public Vector2 shadowSize = new Vector2(0.21f,0.07f);
    public Sprite[] sprites;

    public Variant(Vector2 shadowSize,RectangleHitbox hitbox, float minY, Vector2[] particlePoints,Vector2? shadowPoint,params Sprite[] sprites)
    {
        this.particlePoints = particlePoints;
        this.hitbox = hitbox;
        this.sprites = sprites;
        this.minY = minY;
        this.shadowPoint = shadowPoint.HasValue ? shadowPoint.Value : default;
        this.shadowSize = shadowSize;
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


