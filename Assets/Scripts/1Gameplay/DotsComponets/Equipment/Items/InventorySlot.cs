using System;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent] 
public struct InventorySlot : IBufferElementData, IGetSlot
{
    [GhostField] public int slot;
    [GhostField] public int itemId;    
    [GhostField] public int quantity;
    [GhostField] public float wetness; // 0% - 100%
    [GhostField] public MyColor color;
    [GhostField] public Quality quality; 


    public static readonly InventorySlot Empty = new InventorySlot(){ itemId = -1 };
    
    public InventorySlot(SlotSave save,int slot)
    {
        this.slot = slot;
        this.itemId = save.itemId;
        this.quantity = save.quantity;
        this.wetness = save.wetness;
        this.color = save.color;
        this.quality = save.quality;
    }

    public override string ToString()
    {
        return $"Slot:{slot} ItemID:{itemId} Quantity:{quantity}";
    }

    public void UpdateWetness(float newValue)
    {
        wetness = math.clamp(newValue + wetness, 0.0f, 100.0f);
    }

    public void AddQuantity(int value)
    {
        quantity += value;
    }
    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
    public int GetSlot() { return slot; }
}
