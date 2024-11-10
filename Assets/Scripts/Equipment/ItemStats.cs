
using System;
using UnityEngine;

public class ItemStats
{
    public int itemID { private set; get; } = -1;
    private int _itemCount;
    public int itemCount
    {
        set
        {
            if (value >= 0) _itemCount = value;
            else _itemCount = 1;
        }
        get 
        { 
            return _itemCount; 
        }
    }

    public ItemStats(ItemStats itemStats)
    {
        this.itemID = itemStats.itemID;
        this.itemCount = itemStats.itemCount;
    }
    public ItemStats(int itemID, int itemCount = 1)
    {
        this.itemID = itemID;
        this.itemCount = itemCount;
    }

    public int GetMaxStack()
    {
        return ItemsAsset.instance.GetStackMax(itemID);
    }
    public bool isNull()
    {
        if (itemID != -1) return false;
        else return true;
    }

    public virtual ItemStats Clon()
    {
        return new ItemStats(this);
    }
}
public class DestroyableItem : ItemStats
{
    public int maxLifePonits { private set; get; }
    public int currentLifePoints {private set ; get; }

    public DestroyableItem(int itemID, int maxLifePoints, int itemCount = 1) :base(itemID,itemCount) 
    {
        this.maxLifePonits = maxLifePoints;
        currentLifePoints = this.maxLifePonits;      
    }
    public DestroyableItem(int itemID, int itemCount, int maxLifePoints,int currentLifePoints) : base(itemID, itemCount)
    {
        this.maxLifePonits = maxLifePoints;
        this.currentLifePoints = currentLifePoints;
    }
    public DestroyableItem(DestroyableItem item) : base(item)
    {
        this.maxLifePonits = item.maxLifePonits;
        this.currentLifePoints = item.currentLifePoints;
    }
    public float GetLifePointsInPercent()
    {
        return currentLifePoints/(float)maxLifePonits;
    }

    public override ItemStats Clon()
    {
        return new DestroyableItem(this);
    }

    public void Use()
    {
       if(currentLifePoints > 0) currentLifePoints--;
    }
    
}
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
    public override ItemStats Clon()
    {
        return new RangedWeaponItem(this);
    }

    
}
