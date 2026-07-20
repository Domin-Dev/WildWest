using Unity.Mathematics;
using UnityEngine;
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
    public int hitSound;
    public int incorrectToolSound;
    [Min(0)] public int hitParticles;
    public int durability;
    [Header("Drop")]
    public Drop[] drop;
    //public string texturePath;
    public ToolType toolRequired = ToolType.None;
}
public abstract class VariantItem : BuildingItem
{
    public ObjectVariant[] objectVariants;
    public int2 size = new int2(27, 51);

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
    public Vector2 particlePoint;
    public float minY;
    public Vector2 shadowSize = new Vector2(0.21f,0.07f);
    public Sprite[] sprites;

    public Variant(RectangleHitbox hitbox, float minY, Vector2 particlePoint,params Sprite[] sprites)
    {
        this.particlePoint = particlePoint;
        this.hitbox = hitbox;
        this.sprites = sprites;
        this.minY = minY;
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


