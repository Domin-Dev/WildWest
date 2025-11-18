using UnityEngine;

public static class UIStringsHelper
{ 
    public static string GetColorString(string value,Color color)
    {
        return GetColorString(UnityEngine.ColorUtility.ToHtmlStringRGB(color),color);
    }
    public static string GetColorString(string value, string color)
    {
        return $"<Color=#{color}>{value}</Color>";
    }
}
