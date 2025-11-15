
using System;
using UnityEngine;

public class ItemStats
{
    public int itemID { private set; get; } = -1;
    private int _quantity;
    private float _wetness;
    public Quality quality { private set; get; }

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
    public float wetness
    {
        set
        {
            _wetness = Math.Clamp(value,0f,100f);
        }
        get
        {
            return _wetness;
        }
    }

    public ItemStats(ItemStats itemStats)
    {
        this.itemID = itemStats.itemID;
        this.quantity = itemStats.quantity;
        this.wetness = itemStats.wetness;
        this.quality = itemStats.quality;
    }
    public ItemStats(ItemStats itemStats, int quantity)
    {
        this.itemID = itemStats.itemID;
        this.wetness= itemStats.wetness;
        this.quality = itemStats.quality;
        this.quantity = quantity;
    }
    public ItemStats(int itemID, int itemCount = 1, float wetness = 0, Quality quality = Quality.none)
    {
        this.itemID = itemID;
        this.wetness = wetness;
        this.quality = quality;
        this.quantity = itemCount;
    }
    public ItemStats(InventorySlot inventorySlot)
    {
        itemID = inventorySlot.itemId;
        quantity = inventorySlot.quantity;
        wetness = inventorySlot.wetness;
        quality = inventorySlot.quality;
    }


    public void AddWetness(float wetnessNewItems, int itemQuantity)
    {
        wetness = EQHelperClient.CalculateMixPercentage(itemQuantity,wetnessNewItems,quantity,wetness);
    }
    public float GetFloatWetness()
    {
        return wetness * 0.01f;
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
    public virtual ItemStats Clon(int quantity)
    {
        return new ItemStats(this,quantity);
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



