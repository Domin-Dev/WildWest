using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ContainerItem", menuName = "GameAsset/Items/BuildingItems/Container")]
public class ContainerItem : BuildingObject
{
    public int capacity;
    public ItemID ItemContainer;
}

