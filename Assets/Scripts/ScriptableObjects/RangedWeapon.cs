
using UnityEngine;
[CreateAssetMenu(fileName = "RangedWeapon", menuName = "GameAsset/Items/Weapons/RangedWeapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged Weapon Stats")]
    public float timeToReload;
    public int magazineCapacity;
    public Vector2 aimPoint;
    public Vector2 reloadPoint;
    public AmmoType ammoType;


    public override ItemStats GetItemStats()
    {
        return new RangedWeaponItem(ID,durability, magazineCapacity);
    }
}
