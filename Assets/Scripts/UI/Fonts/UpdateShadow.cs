using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UpdateShadow : MonoBehaviour
{
    private void OnEnable()
    {
        CreateShadow();
    }
    private void CreateShadow()
    {
        Shadow shadow =  this.GetOrAddComponent<Shadow>();
        ShadowSettings shadowSettings = GamePreferences.instance.GetShadowSettings();
        shadow.effectColor = shadowSettings.color;
        shadow.effectDistance = shadowSettings.offset;
    }

}
