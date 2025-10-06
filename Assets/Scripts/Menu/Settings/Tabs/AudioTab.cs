
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



public class AudioTab : SettingsTab
{

    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider soundsVolume;

    private void OnEnable()
    {
        SetSettings(MainSettingsManager.instance.settings);
    }
    private void OnDisable()
    {
    }

    public override void ResetToDefault()
    {
        MainSettingsManager.instance.videoSettings.SetDefaultSettings();
        SetSettings(MainSettingsManager.instance.settings);
    }
    public override void SaveSettings()
    {
        MainSettingsManager.instance.Save();
    }
    private void SetSettings(SettingsData settings)
    {

    }


   
}


