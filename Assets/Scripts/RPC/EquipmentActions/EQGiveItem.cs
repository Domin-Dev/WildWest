using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQGiveItem : IComponentData
{
    public int itemID;
    public int quantity;
    public Entity networkEntity;
}