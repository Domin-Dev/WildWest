
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

    public override string ToString()
    {
        return $"Count: {itemCount} ItemID: {itemID}"; 
    }
}



