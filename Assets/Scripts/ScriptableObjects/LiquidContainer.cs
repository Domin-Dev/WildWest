using System.Collections.Generic;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "LiquidContainer", menuName = "GameAsset/Items/LiquidContainer")]
public class LiquidContainer : Item
{
    public int capacity;

    public override ItemSlot GetItemStats()
    {
        return new LiquidContainerItem(ID,capacity);
    }

}

