
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



public class VideoTab : SettingsTab
{
    [SerializeField] private Toggle fullscreen;
    [SerializeField] private TMP_Dropdown resolution;
    [Space]
    [SerializeField] private Slider fpsLimit;
    [SerializeField] private LocalizeStringEvent fpsText;
    [SerializeField] private LocalizedString unlimitedString;
    [SerializeField] private ListSwitch fontSwitch;


    List<Resolution> selectedResolutions;
    private void OnEnable()
    {
        SetSettings(MainSettingsManager.instance.settings);
        LocalizationSettings.SelectedLocaleChanged += RefreshStrings;
    }
    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= RefreshStrings;
        selectedResolutions.Clear();
    }

    public override void ResetToDefault()
    {
        MainSettingsManager.instance.videoSettings.SetDefaultSettings();
        SetSettings(MainSettingsManager.instance.settings);
    }
    public override void SaveSettings()
    {
    }
    private void SetSettings(SettingsData settings)
    {
        SetUpResolutions(settings);
        SetUpFullScreen(settings);
        SetUpFPSLimit(settings);
        SetUpFont(settings);
    }

    private void SetUpFPSLimit(SettingsData data)
    {
        fpsLimit.onValueChanged.RemoveAllListeners();
        fpsLimit.onValueChanged.AddListener(SetFPSLimit);
        int fps = data.fpsLimit;
        if (fps < 0) fps = (int)fpsLimit.maxValue;
        SetFPSText(fps);
        fpsLimit.value = fps;
    }
    private void SetFPSLimit(float value)
    {
        int fps = Mathf.RoundToInt(value);
        SetFPSText(fps);
    }
    private void SetFPSText(int fps)
    {
        if (fps == fpsLimit.maxValue)
        {
            MainSettingsManager.instance.videoSettings.SetFPSLimit(-1);
            fpsText.StringReference.Arguments = new[] { $" {unlimitedString.GetLocalizedString()}" };
        }
        else
        {
            MainSettingsManager.instance.videoSettings.SetFPSLimit(fps);
            fpsText.StringReference.Arguments = new[] { $" {fps} FPS" };
        }
        fpsText.RefreshString();
    }

    private void SetUpFullScreen(SettingsData settingsData)
    {
        fullscreen.isOn = settingsData.fullScreen;
        fullscreen.onValueChanged.RemoveAllListeners();
        fullscreen.onValueChanged.AddListener((fullscreen) => { MainSettingsManager.instance.videoSettings.SetFullScreen(fullscreen); });
    }
    private void SetUpFont(SettingsData settingsData)
    {
        fontSwitch.SetUpSwitch(UIAssetsManager.instance.GetFontNames(), settingsData.fontIndex);
        fontSwitch.OnChangedValue = null;
        fontSwitch.OnChangedValue += MainSettingsManager.instance.videoSettings.SetFont;
    }
    private void RefreshStrings(Locale obj)
    {
        fontSwitch.RefreshTab(UIAssetsManager.instance.GetFontNames());
    }
    private void SetUpResolutions(SettingsData settings)
    {
        Resolution[] resolutions = Screen.resolutions;
        selectedResolutions = new List<Resolution>();
        resolution.ClearOptions();

        List<string> options = new List<string>();
        int currentValue = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);
                selectedResolutions.Add(resolutions[i]);
                if (resolutions[i].height == settings.resolutionHeight && resolutions[i].width == settings.resolutionWidth)
                {
                    currentValue = i;
                }
            }
        }

        resolution.AddOptions(options);
        resolution.value = currentValue;
        resolution.RefreshShownValue();
        resolution.onValueChanged.RemoveAllListeners();
        resolution.onValueChanged.AddListener((value) =>
        {
            Resolution resolution = selectedResolutions[value];
            MainSettingsManager.instance.videoSettings.SetResolution(resolution);
        });
    }
}


