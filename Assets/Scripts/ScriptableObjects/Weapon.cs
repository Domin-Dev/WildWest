
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "Weapon", menuName = "GameAsset/Items/Weapons/Weapon")]
public class Weapon : Destroyable
{
    [Header("Weapon Stats")]
    public int damage = 5;
    public float cooldown;
    public Sprite weaponImage;

    public Vector2[] hitBoxPoints;

    public float handOffset = 0.07f;
    public Vector2 gripPoint1;
    public Vector2 gripPoint2;

    [Header("Visual effects")]
    public Animation usageAnim;

    public bool twoHanded  => gripPoint2.x != -100;
    public override Sprite GetWorldSprite => weaponImage;
    public override ItemStats GetItemStats()
    {
        return base.GetItemStats();
    }
}

[System.Serializable]
public class Animation
{
    public KeyFrame[] frames;
}