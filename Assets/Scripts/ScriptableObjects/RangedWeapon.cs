
using System.Collections.Generic;
using Unity.Entities.UI;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "RangedWeapon", menuName = "GameAsset/Items/Weapons/RangedWeapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged Weapon Stats")]
    public float timeToReload;
    [Min(0)]public int magazineCapacity;
    [Min(1)]public int bulletCount = 1;
    public float shotSpread;
    public float bulletSpread = 0;
    public AmmoTag ammoTag;
    
    [Header("Visual effects")]
    public List<KeyFrame> shotAnim;
    public List<KeyFrame> reloadAnim; 
    public List<KeyFrame> lostAmmo; 

    public float reloadCooldown => CalculateTime(reloadAnim);


    public float2 aimPoint;
    public float2 reloadPoint;
    public AmmoType oldtype;



    public bool hasMagazine => magazineCapacity > 0;
    public float bulletOffset => bulletSpread / math.max((bulletCount - 1),1);


    public int ammoTagID
    {
        get
        {
            if(ammoTag != null)
                return ammoTag.ID;
            else
                return -1;
        }
    }





    
    public override bool HasContainer(out int containerCapacity, out MandatoryProperties mandatoryProperties, out int mandatoryData)
    {
        containerCapacity = magazineCapacity;
        if(ammoTag != null)
        {
            mandatoryProperties = MandatoryProperties.tag;
            mandatoryData = ammoTag.ID;
        }
        else
        {
            mandatoryProperties = MandatoryProperties.none;
            mandatoryData = 0;
        }

        return hasMagazine;
    }
    public override ItemStats GetItemStats()
    {
        return new RangedWeaponItem(ID,durability, magazineCapacity);
    }
}

