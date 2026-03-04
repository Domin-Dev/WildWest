
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TooltipSystem : MonoBehaviour
{
    [SerializeField] private Tooltip tooltip;

    private static TooltipSystem current;
    private static Timer timer;

    private object displayingObj;

    private void Awake()
    {
        if(current == null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public static void Show(IHaveTooltip tooltip)
    {
        Show(tooltip.GetTooltip());
    }
    public static void Show(TooltipInfo tooltip)
    {
        Show(tooltip.content, tooltip.header,tooltip.displayingObj,tooltip.headerColor);
    }
    public static void Show(string content,string header = "", object displayingObj = null, Color? headerColor = null)
    {
        ShowBase(() => {
            current.displayingObj = displayingObj; 
            current.tooltip.SetText(content, header, headerColor); 
            return true;
        });
    }
    public static void Show(SlotPosition slotPosition, ItemStats itemStats, bool showInstant = false)
    {
        if (itemStats == null) return;
        TooltipInfo tooltipInfo = ItemsAsset.instance.GetTooltipInfo(itemStats);
        tooltipInfo.displayingObj = slotPosition;

        if (showInstant)
            ShowInstant(tooltipInfo);
        else
            Show(tooltipInfo);
    }
    
    public static void Show(Func<TooltipInfo> func)
    {
        ShowBase(() =>
        {
            TooltipInfo tooltipInfo = func();
            if(tooltipInfo != null)
            {
                current.displayingObj = tooltipInfo.displayingObj; 
                current.tooltip.SetText(tooltipInfo.content,tooltipInfo.header, tooltipInfo.headerColor);
            }
            return true;
        });
    }
    

    private static void ShowBase(Func<bool> func, float time = 0.45f)
    {
        if (timer != null) timer.Cancel();
        timer = Timer.Create(time, func);
    }
    public static void ShowInstant(string content, string header = "",object displayingObj = null, Color? headerColor = null)
    {
        current.displayingObj = displayingObj;
        current.tooltip.SetText(content, header,headerColor);
    }
    public static void ShowInstant(TooltipInfo tooltip)
    {
        ShowInstant(tooltip.content, tooltip.header,tooltip.displayingObj, tooltip.headerColor);
    }







    public static bool IsDisplaying(object obj)
    {
        Debug.Log(obj +  ",,, " + current.displayingObj);
        Debug.Log(obj == current.displayingObj);
        return obj.Equals(current.displayingObj);
    }
    public static bool IsDisplaying<T>()
    {
        return current.displayingObj is T;
    }

    public static bool IsSlotPostion(out SlotPosition? slotPosition)
    {
        if(current.displayingObj is SlotPosition)
        {
            slotPosition = current.displayingObj as SlotPosition?;
            return true;
        }
        slotPosition = null;
        return false;
    }

    public static bool IsSelected(SlotPosition slotPosition)
    {
        if(IsSlotPostion(out SlotPosition? s))
        {
            return s.Equals(slotPosition);
        }
        return false;
    }

    public static void Hide()
    {
        current.displayingObj = null;
        if (timer != null) timer.Cancel();
        current.tooltip.Hide();
    }
}
