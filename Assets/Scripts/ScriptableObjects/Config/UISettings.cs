using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public struct BarColor
{
    public Color color;
    public string type;
}


[CreateAssetMenu(fileName = "UISettings", menuName = "GameAsset/Settings/UISettings")]
public class UISettings : ScriptableObject
{
    public Color durabilityBarColor;
    public Color liquidCapacityBarColor;
    public Color dirtyWaterBarColor;
    public Color shelfLifeBarColor;

    public List<BarColor> barColors;


    public List<(Color,Type)> GetColors()
    {
        List<(Color,Type)> colors = new List<(Color,Type)>();
        foreach (var c in barColors)
            colors.Add((c.color, Type.GetType(c.type)));
        return colors;
    }
}

