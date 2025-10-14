using TMPro;
using System;
using UnityEngine;
using Unity.VisualScripting;

[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public class UpdateTextSettings : MonoBehaviour
{

    [SerializeField] private TextType textType = TextType.None;

    private void OnEnable()
    {
        ChangeFont(GamePreferences.instance?.GetCurrentFont());
        GamePreferences.onNewFont += ChangeFont;
    }
    private void OnDisable()
    {
        GamePreferences.onNewFont -= ChangeFont;
    }
    private void ChangeFont(TMP_FontAsset font)
    {
        if(font == null) return;
        var textPro = GetComponent<TMP_Text>();
        textPro.font = font;

        if(textType != TextType.None)
        {
            TextTypePair textSettings = GamePreferences.instance.GetTextSettings(textType);
            if (textSettings == null) return;
            textPro.fontSize = textSettings.fontSize;
        }
    }

}
