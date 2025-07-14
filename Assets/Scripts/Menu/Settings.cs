
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

        fpsLimit.onValueChanged.AddListener(SetFPSLimit);


        if (QualitySettings.vSyncCount == 1 || Application.targetFrameRate == -1)
        {
            SetFPSText((int)fpsLimit.maxValue);
            fpsLimit.value = fpsLimit.maxValue;
        }
        else
        {
            SetFPSText(Application.targetFrameRate);
            fpsLimit.value = Application.targetFrameRate;
        }


    }

    private void CloseSettings()
    {
        SceneManager.UnloadSceneAsync(8);
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    private void SetFPSLimit(float value)
    {
        Debug.Log(Screen.currentResolution.refreshRateRatio);
        Debug.Log(Application.targetFrameRate);
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

    private void Reset()
    {
        
    }
}

