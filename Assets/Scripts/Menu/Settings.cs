
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
    }
    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void Reset()
    {
        
    }
}

