using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;


[System.Serializable]
public struct BarColor
{
    public Color color;
    public string type;
}

[System.Serializable]
public class Property
{
    [SerializeField] private string _name;
    [SerializeField] private LocalizedString _localizedName;
    [SerializeField] private LocalizedString _description;
    [SerializeField] private Color _color;
    [SerializeField] private string _nameIcon;
    [SerializeField] private bool displayEQWindow;
    [SerializeField] private int displayPriority;
    [SerializeField] private bool hideWhenValueIsZero;




    public string name => _name;
    public LocalizedString localizedName => _localizedName;
    public LocalizedString description => _description;
    public Color color => _color;
    public string nameIcon => _nameIcon;
    public bool DisplayEQWindow => displayEQWindow;
    public int DisplayPriority => displayPriority;
    public bool HideWhenValueIsZero => hideWhenValueIsZero;
}




[CreateAssetMenu(fileName = "UISettings", menuName = "GameAsset/Settings/UISettings")]
public class  UIConfig : ScriptableObject
{
    public Color durabilityBarColor;
    public Color liquidCapacityBarColor;
    public Color dirtyWaterBarColor;
    public Color shelfLifeBarColor;

    [SerializeField] private List<BarColor> barColors;
    [SerializeField] private List<Property> properties;


    public List<(Color,Type)> GetColors()
    {
        List<(Color,Type)> colors = new List<(Color,Type)>();
        foreach (var c in barColors)
            colors.Add((c.color, Type.GetType(c.type)));
        return colors;
    }
    public Dictionary<string,Property> GetProperties()
    {
        Dictionary<string,Property> properties = new Dictionary<string, Property>();
        foreach(var p in this.properties)
            properties.Add(p.name,p);
        return properties;
    }


}

