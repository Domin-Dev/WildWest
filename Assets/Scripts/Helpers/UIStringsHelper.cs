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
        return $"<Sprite name={nameIcon}>";
    }
}
