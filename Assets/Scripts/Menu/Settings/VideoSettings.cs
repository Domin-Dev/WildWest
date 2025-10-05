
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

public class VideoSettings: MonoBehaviour 
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
                if (resolutions[i].height == Screen.height && resolutions[i].width == Screen.width)
                {
                    currentValue = i;
                }
            }
        }

        resolution.AddOptions(options);
        resolution.value = currentValue;
        resolution.RefreshShownValue();
        resolution.onValueChanged.AddListener((value) => {
            Resolution resolution = selectedResolutions[value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        });
        fullscreen.isOn = Screen.fullScreen;
        fullscreen.onValueChanged.AddListener((fullscreen) => { SetFullscreen(fullscreen); });
        fpsLimit.onValueChanged.AddListener(SetFPSLimit);

        if (QualitySettings.vSyncCount == 0 && Application.targetFrameRate == -1)
        {
            SetFPSText((int)fpsLimit.maxValue);
            fpsLimit.value = fpsLimit.maxValue;
        }
        else
        {
            int fps = Application.targetFrameRate;
            var refreshRate = Screen.currentResolution.refreshRateRatio;
            if (QualitySettings.vSyncCount == 1)
                fps = (int)((float)refreshRate.numerator / refreshRate.denominator);
            else if(QualitySettings.vSyncCount == 2)
                fps = (int)((float)refreshRate.numerator / refreshRate.denominator / 2f);


            SetFPSText(fps);
            fpsLimit.value = fps;
        }



        fontSwitch.SetUpSwitch(UIAssetsManager.instance.GetFontNames(),GamePreferences.instance.GetCurrentIndexFont());
        fontSwitch.OnChangedValue += SetFont;

        LocalizationSettings.SelectedLocaleChanged += RefreshStrings;
    }
    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= RefreshStrings;
    }

    private void RefreshStrings(Locale obj)
    {
        fontSwitch.RefreshTab(UIAssetsManager.instance.GetFontNames());
    }

    private void SetFont(object sender, int e)
    {
        GamePreferences.instance.NewFont(e);
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    private void SetFPSLimit(float value)
    {
        int fps = Mathf.RoundToInt(value);
        SetFPSText(fps);
        QualitySettings.vSyncCount = 0;
    }
    private void SetFPSText(int fps)
    {
        if (fps == fpsLimit.maxValue)
        {
            Application.targetFrameRate = -1;
            fpsText.StringReference.Arguments = new[]{ $" {unlimitedString.GetLocalizedString()}" };
        }
        else
        {
            Application.targetFrameRate = fps;
            fpsText.StringReference.Arguments = new[] { $" {fps} FPS" }; 
        }
        fpsText.RefreshString();
    }
    public void SetDefaultSettings()
    {    
        resolution.value = selectedResolutions.Count - 1;
        resolution.RefreshShownValue();

        SetFullscreen(true);
        fullscreen.isOn = true;

        var refreshRate = Screen.currentResolution.refreshRateRatio;
        float fps = ((float)refreshRate.numerator / refreshRate.denominator);
        SetFPSLimit(fps);
        fpsLimit.value = fps;
    }

}

