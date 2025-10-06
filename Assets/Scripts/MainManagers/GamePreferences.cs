using System;
using TMPro;
using UnityEngine;

public class GamePreferences : MonoBehaviour
{
    public static GamePreferences instance;
    public static Action<TMP_FontAsset> onNewFont;
    public int fontIndex = 0;
    public TMP_FontAsset GetCurrentFont()
    {
        return UIAssetsManager.instance.fonts[fontIndex].font;
    }
    public int GetCurrentIndexFont()
    {
        return fontIndex;
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
