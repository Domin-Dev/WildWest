using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQGiveItem : IComponentData
{
    public InventorySlot item;
    public float barValue;
    public Entity networkEntity;


}