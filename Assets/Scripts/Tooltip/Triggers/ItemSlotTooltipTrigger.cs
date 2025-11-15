using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[DisallowMultipleComponent]
[RequireComponent(typeof(IGetSlotPosition))]
public class ItemSlotTooltipTrigger :  TooltipTriggerBase
{
    public override void Tooltip(PointerEventData eventData)
    {
        SlotPosition slotPosition = GetComponent<IGetSlotPosition>().GetSlotPosition();
        ItemStats item = NewEquipmentManager.instance.ReadItemStats(slotPosition);
        if (item == null) return;  
        TooltipSystem.Show(slotPosition,item);
    }
}
