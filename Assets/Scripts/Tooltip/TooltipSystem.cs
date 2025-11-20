
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TooltipSystem : MonoBehaviour
{
    [SerializeField] private Tooltip tooltip;

    [Space]
    [SerializeField] private LocalizedString wetness;
    [SerializeField] private LocalizedString quality;
    [SerializeField] private LocalizedString maxStack;


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

    public static void Append(StringBuilder content, string colorString, string fieldName,string iconName, params string[] value)
    {
        string joined = string.Join(" ", value);
        content.Append($"{(content.Length > 0 ? "\n" : "")}<Color=#{colorString}>{UIStringsHelper.GetSpriteIcon(iconName)} {fieldName}:</Color> {joined}");
    }
    private static TooltipInfo GetTooltip(ItemStats itemStats,SlotPosition slotPosition)
    {
        var data = ItemsAsset.instance.GetTooltipInfo(itemStats.itemID);
        StringBuilder content = new StringBuilder();
        StringBuilder header = new StringBuilder();
        Color? hColor = UIAssetsManager.instance.GetQualityColor(itemStats.quality);


        header.Append(data.header);
        if (itemStats.quality != Quality.none)
            header.Append($" [ {itemStats.quality.ToString().ToUpper()} ]");

        content.Append(data.content);

        var tags = ItemsAsset.instance.GetItemTags(itemStats.itemID);
        if (tags.Length > 0)
        {   
            Append(content, GamePreferences.instance.highlightColorStr,LocalizationSettings.StringDatabase.GetLocalizedString(Translations.eqTable, "Tags"),"Tag", tags);
        }
        if (itemStats is ItemWithBar)
        {
            ItemWithBar barValue = (ItemWithBar)itemStats;
            string bar = ItemsAsset.instance.GetBarName(itemStats.itemID);
            Append(content, UIManager.instance.GetColorHexStringForItem(itemStats.itemID), LocalizationSettings.StringDatabase.GetLocalizedString(Translations.eqTable, bar, fallbackBehavior: FallbackBehavior.UseProjectSettings),bar,barValue.current.ToString("F2") + "/" + barValue.maxValue.ToString("F2"));
        }

        if (itemStats.wetness >= 0.01)
            Append(content, "67CCFF", current.wetness.GetLocalizedString(),"Water", itemStats.wetness.ToString("F2") + " %");
        Append(content, "F09A42", current.maxStack.GetLocalizedString(),"MaxStack", itemStats.GetMaxStack().ToString());
      
        return new TooltipInfo(content.ToString(), header.ToString(),slotPosition,hColor);
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
            return false;
        });
    }
    public static void Show(SlotPosition slotPosition, ItemStats itemStats, bool showInstant = false)
    {
        if (itemStats == null) return;
        TooltipInfo tooltipInfo = GetTooltip(itemStats,slotPosition);
        if (showInstant)
            ShowInstant(tooltipInfo);
        else
            Show(tooltipInfo);
    }
    
    //public static void Show(Container container)
    //{
    //    TooltipInfo tooltipInfo = GetTooltip(itemStats, slotPosition);
    //    Show(tooltipInfo);
    //}




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
    public static void Hide()
    {
        current.displayingObj = null;
        if (timer != null) timer.Cancel();
        current.tooltip.Hide();
    }
}
