using System.Text;
using UnityEngine;

public static class UIStringsHelper
{ 
    public static string GetColorfulString(string value,Color color)
    {
        return GetColorString(value,UnityEngine.ColorUtility.ToHtmlStringRGB(color));
    }
     public static string GetStringWithDefaultColor(string value)
    {
        return GetColorfulString(value,GamePreferences.instance.highlightColor);
    }
    public static string GetColorString(string value, string color)
    {
        return $"<Color=#{color}>{value}</Color>";
    }

    public static string GetSpriteIcon(string nameIcon)
    {
        if(string.IsNullOrEmpty(nameIcon)) return null;
        return $"<Sprite name={nameIcon}>";
    }


    public static void Append(StringBuilder content, string colorString, string fieldName,string iconName, params string[] value)
    {
        string joined = string.Join(" ", value);
        content.Append($"{(content.Length > 0 ? "\n" : "")}<Color=#{colorString}>{GetSpriteIcon(iconName)} {fieldName}:</Color> {joined}");
    }
    public static void Append(StringBuilder content,Property property, params string[] value)
    {
        Append(content, ColorUtility.ToHtmlStringRGB(property.color),property.localizedName.GetLocalizedString(),property.nameIcon, value);
    }

    public static void Append(StringBuilder content,Property property, int value)
    {
        Append(content, ColorUtility.ToHtmlStringRGB(property.color),property.localizedName.GetLocalizedString(),property.nameIcon,((value > 0 ? "+" : "" ) + value.ToString()));
    }

    public static void AppendArgs(StringBuilder content,Property property, params string[] args)
    {
        content.Append($"{(content.Length > 0 ? "\n" : "")}{GetSpriteIcon(property.nameIcon)} {property.localizedName.GetLocalizedString(args)}");
    }
}


