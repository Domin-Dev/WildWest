
using System.Text;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Localization;

public class TooltipSystem : MonoBehaviour
{
    [SerializeField] private Tooltip tooltip;

    [Space]
    [SerializeField] private LocalizedString wetness;
    [SerializeField] private LocalizedString quality;
    [SerializeField] private LocalizedString maxStack;
    private static TooltipSystem current;

    private static Timer timer;

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

    public static void Show(ItemStats itemStats)
    {
        var data = ItemsAsset.instance.GetTooltipInfo(itemStats.itemID);
        StringBuilder content = new StringBuilder();
        StringBuilder header = new StringBuilder();
        Color? hColor = UIAssetsManager.instance.GetQualityColor(itemStats.quality);




        header.Append(data.header);
        if (itemStats.quality != Quality.none)
        {
            header.Append($" [ {itemStats.quality.ToString().ToUpper()} ]");
        }

        content.Append(data.content);
        if(itemStats.wetness > 0)
            Append(content,"67CCFF",current.wetness.GetLocalizedString(),itemStats.wetness+ " %","Water");
        Append(content, "F09A42", current.maxStack.GetLocalizedString(), itemStats.GetMaxStack().ToString(), "MaxStack");



        Show(content.ToString(), header.ToString(),hColor );
    }

    private static void Append(StringBuilder content,string colorString,string fieldName,string value)
    {
        content.Append($"\n<Color=#{colorString}>{fieldName}:</Color> {value}");
    }
    private static void Append(StringBuilder content, string colorString, string fieldName, string value, string iconName)
    {
        content.Append($"\n<Color=#{colorString}><Sprite name={iconName}> {fieldName}:</Color> {value}");
    }
    public static void Show(TooltipInfo tooltip)
    {
        Debug.Log(tooltip.header);
        Show(tooltip.content, tooltip.header);
    }
    public static void Show(string content,string header = "",Color? headerColor = null)
    {
        if (timer != null) timer.Cancel(); 
        timer = Timer.Create(0.45f,() => { current.tooltip.SetText(content, header, headerColor); return false;});
    }
    public static void ShowInstant(string content, string header = "")
    {
       current.tooltip.SetText(content, header);
    }
    public static void Hide()
    {
        if(timer != null) timer.Cancel();
        current.tooltip.Hide();
    }



}
