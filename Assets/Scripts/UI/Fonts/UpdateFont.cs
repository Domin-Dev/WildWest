using TMPro;
using System;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public class UpdateFont : MonoBehaviour
{
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
        GetComponent<TMP_Text>().font = font;
    }

}
