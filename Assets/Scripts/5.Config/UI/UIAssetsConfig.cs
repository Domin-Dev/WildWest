using System;
using System.Collections.Generic;
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
}

[System.Serializable]
public class Backgrounds
{
    public Background wood;
    public Background iron;
    public Background woodenFrame;
    
}


// DOTO create config class
[CreateAssetMenu(fileName = "UIAssetsConfig", menuName = "GameAsset/Settings/UIAssetsConfig")]
public class  UIAssetsConfig : MonoBehaviour
{
    private static UIAssetsConfig _instance;
    public static UIAssetsConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<UIAssetsConfig>("Config/UIAssetsConfig");

            return _instance;
        }
    }


    [SerializeField] public Backgrounds backgrounds{private set;get;}

    [SerializeField] public List<QualityData> qualitySprites;


    [Header("Headers")]
    [SerializeField] public Sprite yellowHeader;
    [SerializeField] public Sprite whiteHeader;
    [Header("Materials")]
    [SerializeField] public Material UIHeadMaterial;
    [SerializeField] public Material UIColorItem;
    [Header("Rewards")]
    [SerializeField] public Sprite bronzeBackgroundUI;
    [SerializeField] public Sprite silverBackgroundUI;
    [SerializeField] public Sprite goldBackgroundUI;
    [Header("Backgrounds")]
    [SerializeField] public Sprite woodBackgroundUI;
    [SerializeField] public Sprite ironBackgroundUI;
    [Header("Frames")]
    [SerializeField] public Sprite woodenFrameUI;
    [SerializeField] public Sprite blackFrameUI;
    [Header("Selected")]
    [SerializeField] public Sprite selectedWoodBackgroundUI;
    [SerializeField] public Sprite selectedIronBackgroundUI;
    [Header("Pressed")]
    [SerializeField] public Sprite goldPressedBackgroundUI;
    [Space]
    [SerializeField] public Sprite ironBarsUI;

    public string[] GetFontNames()
    {
        List<string> names = new List<string>();
        foreach (var item in fonts)
        {
            names.Add(item.name.GetLocalizedString());
        }
        return names.ToArray();
    }
    public Sprite GetQualitySprite(Quality quality)
    {
        foreach (var item in qualitySprites)
        {
            if (item.quality == quality)
                return item.sprite;
        }
        return null;
    }
    public Color? GetQualityColor(Quality quality)
    {
        foreach (var item in qualitySprites)
        {
            if (item.quality == quality)
                return item.color;
        }
        return null;
    }
    public void ChangeButtonToIron(Button button)
    {
        var states = button.spriteState;
        states.pressedSprite = goldPressedBackgroundUI;

        states.selectedSprite = selectedIronBackgroundUI;
        states.highlightedSprite = selectedIronBackgroundUI;

        button.spriteState = states;
        button.image.sprite = ironBackgroundUI;
    }
    public void ChangeButtonToWood(Button button)
    {
        var states = button.spriteState;
        states.pressedSprite = goldPressedBackgroundUI;

        states.selectedSprite = selectedWoodBackgroundUI;
        states.highlightedSprite = selectedWoodBackgroundUI;

        button.spriteState = states;
        button.image.sprite = woodBackgroundUI;
    }
}



