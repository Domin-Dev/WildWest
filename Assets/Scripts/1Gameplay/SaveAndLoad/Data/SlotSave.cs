
using System;
using Unity.Collections;
using Unity.VisualScripting;

[System.Serializable]
public struct SlotSave 
{
    public int itemId;    
    public int quantity;
    public float wetness;
    public MyColor color;
    public Quality quality; 

    public SlotSave(InventorySlot slot)
    {
        this.itemId = slot.itemId;
        this.quantity = slot.quantity;
        this.wetness = slot.wetness;
        this.color = slot.color;
        this.quality = slot.quality;
    }

    public void Add(int quantity)
    {
        this.quantity += quantity;
    }
}
