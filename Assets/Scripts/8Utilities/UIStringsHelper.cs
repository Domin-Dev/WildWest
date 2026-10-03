using System.Text;
using UnityEngine;

public static class UIStringsHelper
{ 
    public static string GetColorfulString(string value,Color color)
    {
        return GetColorfulString(value,UnityEngine.ColorUtility.ToHtmlStringRGB(color));
    }
    public static string GetColorfulString(string value, string color)
    {
        return $"<Color=#{color}>{value}</Color>";
    }
    public static string GetStringWithDefaultColor(string value)
    {
        return GetColorfulString(value,GamePreferences.instance.highlightColor);
    }
    public static string GetSpriteIcon(string nameIcon)
    {
        if(string.IsNullOrEmpty(nameIcon)) return null;
        return $"<Sprite name={nameIcon}>";
    }


    public static void Append(StringBuilder content, string colorString, string fieldName,string iconName, params string[] value)
    {
        content.Append($"{(content.Length > 0 ? "\n" : "")}{GetPropertyString(colorString,fieldName,iconName,value)}");
    }
    public static void Append(StringBuilder content, Property property,params string[] value)
    {
        content.Append($"{(content.Length > 0 ? "\n" : "")}{GetPropertyString(property,value)}");
    }

    public static string GetPropertyString<T>(Property property,params T[] value)
    {
        return GetPropertyString(ColorUtility.ToHtmlStringRGB(property.color), property.localizedName.GetLocalizedString(),property.nameIcon, value);
    }
    public static string GetPropertyString<T>(Property property,Color color,params T[] value) 
    {
        return GetPropertyString(ColorUtility.ToHtmlStringRGB(color), property.localizedName.GetLocalizedString(),property.nameIcon, value);
    }
    public static string GetPropertyString<T>(string colorString, string fieldName,string iconName,params T[] value)
    {
        string joined = string.Join(" ", value);
        return $"<Color=#{colorString}>{GetSpriteIcon(iconName)} {fieldName} :</Color> {joined}"; 
    }

    public static void Append(StringBuilder content,Property property, int value)
    {
        Append(content,property,((value > 0 ? "+" : "" ) + value.ToString()));
    }

    public static void AppendArgs(StringBuilder content,Property property, params string[] args)
    {
        content.Append($"{(content.Length > 0 ? "\n" : "")}{GetSpriteIcon(property.nameIcon)} {property.localizedName.GetLocalizedString(args)}");
    }
}


