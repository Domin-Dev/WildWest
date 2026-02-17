
using System;
using Unity.Collections;
using Unity.VisualScripting;

[System.Serializable]
public struct BarDataSave 
{
    public float value;
    public float maxValue;
    public BarDataSave(ItemBarData slot)
    {
        this.value = slot.value;
        this.maxValue = slot.maxValue;
    }
}
