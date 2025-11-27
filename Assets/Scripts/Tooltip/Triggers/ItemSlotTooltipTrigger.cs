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

        Debug.Log("!!!!!!!!!!!!!!dziala!!!!!!!!!!!!!!!!!1");

        if (item == null) 
            TooltipSystem.Show(NewEquipmentManager.instance.GetTooltipInfo(slotPosition.containerIndex));
 
        TooltipSystem.Show(slotPosition,item);
    }
}
