using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

[System.Serializable]
public class FontData
{
    public LocalizedString name;
    public TMP_FontAsset font;
}

[System.Serializable]
public class QualityData
{
    public Quality quality;
    public Sprite sprite;
    public Color color;
}

[System.Serializable]
public class Background
{
    public Sprite Sprite;
    public Sprite SelectedSprite;
    public bool UseDefaultPressedSprite;

    [HideIf("UseDefaultPressedSprite")]
    [AllowNesting] 
    [SerializeField] 
    private Sprite pressedSprite;
    public Sprite PressedSprite => UseDefaultPressedSprite ? UIConfig.Instance.Backgrounds.DefaultPressedSprite : pressedSprite;
}

[System.Serializable]
public class Backgrounds
{
    public enum BackgroundType
    {
        Wood,
        Iron,
        Bronze,
        Silver,
        Gold,
        Red,
        Green,
        Blue,
        WoodenFrame,
        IronFrame,
        IronBlackFrame,
    }
    public Background wood;
    public Sprite DefaultPressedSprite;
}

[System.Serializable]
public class Materials
{
    public Material HeadIcon;
    public Material ColorItem;
}

[System.Serializable]
public class Headers
{
    public Sprite yellowHeader;
    public Sprite whiteHeader;
}
[System.Serializable]
public class ProfileCovers 
{
    [SerializeField] public Sprite IronBars;
}

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

[System.Serializable]
public class UIThermometerConfig
{
    public Color BelowZeroThermometerColor;
    public Color AboveZeroThermometerColor;
    [Range(-60,60)]
    public float minTemperature;
    [Range(-60,60)]
    public float maxTemperature;
}

