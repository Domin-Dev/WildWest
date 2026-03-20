
using System.Collections.Generic;
using Unity.Entities.UI;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(fileName = "RangedWeapon", menuName = "GameAsset/Items/Weapons/RangedWeapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged Weapon Stats")]
    public float timeToReload;
    public int magazineCapacity;
    [Min(1)]public int bulletCount = 1;
    public float shotSpread;
    public float bulletSpread = 0;

    [Header("Visual effects")]
    public List<KeyFrame> keyFrames;

    [Header("Sounds")]
    public AudioClip shotSound;





    public float2 aimPoint;
    public float2 reloadPoint;
    public AmmoType ammoType;


    public float bulletOffset => bulletSpread / math.max((bulletCount - 1),1);

    public override ItemStats GetItemStats()
    {
        return new RangedWeaponItem(ID,durability, magazineCapacity);
    }
}
