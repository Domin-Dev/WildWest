
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings: MonoBehaviour 
{
    [SerializeField] private Toggle fullscreen;
    [SerializeField] private TMP_Dropdown resolution;

    [SerializeField] private Button closeSettings;
    [SerializeField] private Button setDefaultSettings;
    [Space]
    [SerializeField] private Slider fpsLimit;
    [SerializeField] private TextMeshProUGUI fpsLimitText;


    List<Resolution> selectedResolutions;
    private void Awake()
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

        closeSettings.onClick.AddListener(CloseSettings);
        setDefaultSettings.onClick.AddListener(SetDefaultSettings);

        fpsLimit.onValueChanged.AddListener(SetFPSLimit);


        Debug.Log("App :" + QualitySettings.vSyncCount + ", fps " + Application.targetFrameRate);

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


    }

    private void CloseSettings()
    {
        UIManager.instance.UnloadScene(8);
        UIManager.instance.LoadScene(GameInfo.instance.lastLoadedScene);
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
            fpsLimitText.text = "Unlimited";
        }
        else
        {
            Application.targetFrameRate = fps;
            fpsLimitText.text = fps + " FPS";
        }
    }
    private void SetDefaultSettings()
    {    
        resolution.value = selectedResolutions.Count - 1;
        resolution.RefreshShownValue();

        SetFullscreen(true);
        fullscreen.isOn = true;

        var refreshRate = Screen.currentResolution.refreshRateRatio;
        float fps = ((float)refreshRate.numerator / refreshRate.denominator);
        SetFPSLimit(fps);
    }
}

