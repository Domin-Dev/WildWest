
using System;
using UnityEngine;




public interface IReadOnlyItemStats
{
    public int itemID { get; }
    public Quality quality { get; }
    public int quantity { get; }
    public float wetness { get; }
    public Color? color { get; }
}

public class ItemStats : IReadOnlyItemStats
{
    public int itemID { private set; get; } = -1;
    public Quality quality { private set; get; }
    private int _quantity;
    private float _wetness;
    private Color? _color;


    public Color? color => _color; 
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
        this._color = itemStats.color;
    }
    public ItemStats(ItemStats itemStats, int quantity)
    {
        this.itemID = itemStats.itemID;
        this.wetness= itemStats.wetness;
        this.quality = itemStats.quality;
        this.quantity = quantity;
        this._color = itemStats.color;
    }
    public ItemStats(int itemID, int itemCount = 1, float wetness = 0, Quality quality = Quality.none, Color? color = null)
    {
        this.itemID = itemID;
        this.wetness = wetness;
        this.quality = quality;
        this.quantity = itemCount;
        this._color = color;
    }
    public ItemStats(InventorySlot inventorySlot)
    {
        itemID = inventorySlot.itemId;
        quantity = inventorySlot.quantity;
        wetness = inventorySlot.wetness;
        quality = inventorySlot.quality;
        _color = inventorySlot.color.ConvertToUnityColor();
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



