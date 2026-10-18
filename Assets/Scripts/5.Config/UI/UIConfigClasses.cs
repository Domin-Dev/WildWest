using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using EditorAttributes;

[System.Serializable]
public class FontData
{
    public LocalizedString name;
    public TMP_FontAsset font;
}

[System.Serializable]
public class QualityData
{
    [AssetPreview(64f,64f)]public Sprite sprite;
    public Color color;
}

[System.Serializable]
public class Background
{
    [AssetPreview(32f,32f)]
    public Sprite Sprite;
    [AssetPreview(32f,32f)]
    [SerializeField] private Sprite selectedSprite;
    public bool UseDefaultPressedSprite;
    [HideField(nameof(UseDefaultPressedSprite))]
    [AssetPreview(32f,32f)]
    [SerializeField] private Sprite pressedSprite;
    public Sprite PressedSprite => UseDefaultPressedSprite ? UIConfig.Instance.Backgrounds.DefaultPressedSprite : pressedSprite;
    public Sprite SelectedSprite => selectedSprite == null ? Sprite : selectedSprite;
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
    [SerializeField] public Dictionary<BackgroundType,Background> backgrounds;
    [AssetPreview(32f,32f)]
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

