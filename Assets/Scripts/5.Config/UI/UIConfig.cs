using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UIConfig", menuName = "GameAsset/UIConfig")]
public class UIConfig : Config<UIConfig>
{ 
    [SerializeField] public List<BarColor> barColors;
    [SerializeField] public Dictionary<string,Property> Properties;
    [SerializeField] public List<FontData> Fonts;
    [SerializeField] public UIThermometerConfig ThermometerConfig;
    [SerializeField] public Materials Materials;
    [SerializeField] public Dictionary<Quality,QualityData> Quality;
    [SerializeField] public Headers Headers;
    [SerializeField] public Backgrounds Backgrounds;
    [SerializeField] public ProfileCovers ProfileCovers;
    
    public static string[] GetFontNames()
    {
        List<string> names = new List<string>();
        foreach (var item in Instance.Fonts)
            names.Add(item.name.GetLocalizedString());
        return names.ToArray();
    }
    public static List<(Color,Type)> GetColors()
    {
        List<(Color,Type)> colors = new List<(Color,Type)>();
        foreach (var c in Instance.barColors)
            colors.Add((c.color, Type.GetType(c.type)));
        return colors;
    }
    public static Property GetProperty(string propertyName)
    {
        if(Instance.Properties.TryGetValue(propertyName,out var value))
            return value;
        else
            return null;
    }
    public static Sprite GetQualitySprite(Quality quality)
    {
        if(Instance.Quality.TryGetValue(quality,out var value))
            return value.sprite;
        else
            return null;
    }
    public static Color? GetQualityColor(Quality quality)
    {
        if(Instance.Quality.TryGetValue(quality,out var value))
            return value.color;
        else
            return null;
    }
    public static void ChangeButton(Button button,Backgrounds.BackgroundType type)
    {
        var states = button.spriteState;
        var background = GetBackground(type);
        states.pressedSprite = background.PressedSprite;
        states.selectedSprite =  background.SelectedSprite;
        states.highlightedSprite = background.SelectedSprite;
        button.spriteState = states;
        button.image.sprite = background.Sprite;
    }
    public static Background GetBackground(Backgrounds.BackgroundType type)
    {
        if(Instance.Backgrounds.backgrounds.TryGetValue(type,out var value))
            return value;
        else
            return null;
    }
    public static Sprite GetBackgroundSprite(Backgrounds.BackgroundType type)
    {
        var background = GetBackground(type);
        return background.Sprite;
    }
}
