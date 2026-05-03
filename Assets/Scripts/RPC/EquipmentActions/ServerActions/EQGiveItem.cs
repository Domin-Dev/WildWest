using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQGiveItem : IComponentData
{
    public InventorySlot item;
    public float barValue;
    public Entity networkEntity;

    public EQGiveItem(InventorySlot slot)
    {
        this.item = slot;
        barValue = 0;
        networkEntity = Entity.Null;
    }
}