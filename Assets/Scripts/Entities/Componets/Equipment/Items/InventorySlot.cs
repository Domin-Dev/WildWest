using System;
using Unity.Entities;
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

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
    public int GetSlot() { return slot; }
}

public struct MyColor 
{
    public byte R;
    public byte G;
    public byte B;
    public bool hasColor;

    public MyColor(byte R,byte G,byte B)
    {
        this.R = R;
        this.G = G;
        this.B = B;
        this.hasColor = true;
    }
    
    public Color? ConvertToUnityColor()
    {
        if(!hasColor) return null;
        return new Color(R/255f,G/255f,B/255f);
    }
    public static bool TryParseHex(string hex, out MyColor color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(hex))
            return false;

        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length != 6)
            return false;

        try
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            color = new MyColor(r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
public enum Quality : byte
{
    none = 0,
    terrible = 1,
    poor = 2,
    normal = 3,
    good = 4,
    masterful = 5,
    legendary = 6,
}
public struct ContainerLoaded : IComponentData
{
}
