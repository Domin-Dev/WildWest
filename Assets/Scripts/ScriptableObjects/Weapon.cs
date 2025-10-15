
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "Weapon", menuName = "GameAsset/Items/Weapons/Weapon")]
public class Weapon : Destroyable
{
    [Header("Weapon Stats")]
    public Sprite weaponImage;

    public Vector2[] hitBoxPoints;
    public Vector2 gripPoint1;
    public Vector2 gripPoint2;


    public int damage = 5;
    public int cooldown;

    public override ItemStats GetItemStats()
    {
        return base.GetItemStats();
    }
}
