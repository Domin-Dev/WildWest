using UnityEngine;

public struct MyColor 
{
    public byte R;
    public byte G;
    public byte B;
    public bool hasColor;

    public MyColor(byte R,byte G,byte B)
    {
        this.R = R;
        this.G = G;
        this.B = B;
        this.hasColor = true;
    }
    
    public Color? ConvertToUnityColor()
    {
        if(!hasColor) return null;
        return new Color(R/255f,G/255f,B/255f);
    }
    public static bool TryParseHex(string hex, out MyColor color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(hex))
            return false;

        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length != 6)
            return false;

        try
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            color = new MyColor(r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
