
using System;
using UnityEngine;

public class ItemSlot
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

    public ItemSlot(ItemSlot itemStats)
    {
        this.itemID = itemStats.itemID;
        this.quantity = itemStats.quantity;
    }
    public ItemSlot(int itemID, int itemCount = 1)
    {
        this.itemID = itemID;
        this.quantity = itemCount;
    }

    public ItemSlot(InventorySlot inventorySlot)
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


    public virtual ItemSlot Clon()
    {
        return new ItemSlot(this);
    }

    public override string ToString()
    {
        return $"Count: {quantity} ItemID: {itemID}"; 
    }
}



