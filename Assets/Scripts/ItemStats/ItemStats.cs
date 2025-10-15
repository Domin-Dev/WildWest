
using System;
using UnityEngine;

public class ItemStats
{
    public int itemID { private set; get; } = -1;
    private int _quantity;
    public int quantity
    {
        set
        {
            if (value >= 0) _quantity = value;
            else _quantity = 1;
        }
        get 
        { 
            return _quantity; 
        }
    }

    public ItemStats(ItemStats itemStats)
    {
        this.itemID = itemStats.itemID;
        this.quantity = itemStats.quantity;
    }
    public ItemStats(int itemID, int itemCount = 1)
    {
        this.itemID = itemID;
        this.quantity = itemCount;
    }

    public ItemStats(InventorySlot inventorySlot)
    {
        itemID = inventorySlot.ItemId;
        quantity = inventorySlot.quantity;
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
        return $"Count: {quantity} ItemID: {itemID}"; 
    }
}



