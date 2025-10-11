using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TextType
{ 
    None,
    Button,
    Header,
    Normal
}

[Serializable]
public class TextTypePair
{
    public TextType textType;
    public int fontSize;
}

[Serializable]
public class ShadowSettings
{
    public Color color;
    public Vector2 offset;
}


public class GamePreferences : MonoBehaviour
{
    public static GamePreferences instance;
    public static Action<TMP_FontAsset> onNewFont;
    private int fontIndex = 0;

    [SerializeField] private Color shadowColor; 
    [SerializeField] private ShadowSettings shadowSettings;
    [SerializeField] private List<TextTypePair> titleFontSizes;

    public TMP_FontAsset GetCurrentFont()
    {
        return UIAssetsManager.instance.fonts[fontIndex].font;
    }
    public int GetCurrentIndexFont()
    {
        return fontIndex;
    }
    public TextTypePair GetTextSettings(TextType textType)
    {
        foreach (var pair in titleFontSizes)
        {
            if(pair.textType == textType)
            {
                return pair;
            }
        }
        return null;
    }
    public ShadowSettings GetShadowSettings()
    {
        return shadowSettings;
    }
    public void NewFont(int index)
    {
        if (UIAssetsManager.instance.fonts.Count < index || index < 0) return;
        fontIndex = index;
        onNewFont?.Invoke(GetCurrentFont());
    }
    private void Awake()
    {
        if (instance == null)
        { 
            instance = this;
            DontDestroyOnLoad(gameObject);    
        }
        else
            Destroy(gameObject);
    }
}
