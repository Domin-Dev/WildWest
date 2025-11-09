
using System;
using UnityEngine;

public class RangedWeaponItem : DestroyableItem
{
    public int magazineCapacity { private set; get; }
    public int currentAmmoCount { private set; get; }

    public RangedWeaponItem(int itemID, int maxLifePoints, int currentLifePoints, int magazineCapacity, int itemCount = 1, int currentAmmoCount = 0) : base(itemID, itemCount, maxLifePoints, currentLifePoints)
    {
        this.magazineCapacity = magazineCapacity;
        this.currentAmmoCount = currentAmmoCount;
    }

    public RangedWeaponItem(int itemID, int maxLifePoints, int magazineCapacity, int itemCount = 1, int currentAmmoCount = 0) : base(itemID, maxLifePoints, itemCount)
    {
        this.magazineCapacity = magazineCapacity;
        this.currentAmmoCount = currentAmmoCount;
    }
    public RangedWeaponItem(RangedWeaponItem item) : base(item)
    {
        this.magazineCapacity = item.magazineCapacity;
        this.currentAmmoCount = item.currentAmmoCount;
    }
    public bool CanReload()
    {
        return currentAmmoCount < magazineCapacity;
    }


    public void Shot()
    {
        currentAmmoCount--;
    }
    public int Reload(int ammoCount)
    {
        int toFull = magazineCapacity - currentAmmoCount;

        if (ammoCount >= toFull)
        {
            currentAmmoCount = magazineCapacity;
            return toFull;
        }
        else
        {
            currentAmmoCount += ammoCount;
            return ammoCount;
        }
    }
    public bool HasAmmo()
    {
        return currentAmmoCount > 0;
    }

    public int ToFullMagazine()
    {
        return magazineCapacity - currentAmmoCount;
    }
    public override ItemStats Clon(int q)
    {
        return new RangedWeaponItem(this);
    }
}

    