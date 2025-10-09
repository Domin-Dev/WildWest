
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class KeyboardControlsTab : SettingsTab
{
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }

    public override void ResetToDefault()
    {
        MainSettingsManager.instance.controlsSettings.SetDefaultSettings("Keyboard&Mouse");
    }
    public override void SaveSettings()
    {
    }
}


