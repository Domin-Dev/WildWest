using Unity.Mathematics;
using UnityEngine;

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
    public int durability;
    [Header("Drop")]
    public Drop[] drop;
    public string texturePath;
    public ToolType toolRequired = ToolType.None;
}
public abstract class VariantItem : BuildingItem
{
    public GameObject HitParticles;
    public ObjectVariant[] objectVariants;
    public int shadowPixels = 19;
    public int2 size = new int2(27, 51);
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
    public Vector2[] hitbox;
    public Vector2[] CoveringPoints;
    public Vector2[] objectPoints;
    public Vector2 particlePoint;
    public float minY;
    public Sprite sprite;

    public Variant(Vector2[] hitbox, Sprite sprite, float minY, Vector2 particlePoint)
    {
        this.particlePoint = particlePoint;
        this.hitbox = hitbox;
        this.sprite = sprite;
        this.minY = minY;
    }

    public Variant Clone()
    {
        return new Variant(hitbox, sprite, minY,particlePoint);
    }
}
