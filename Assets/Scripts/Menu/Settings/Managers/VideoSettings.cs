using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class VideoSettings
{
    private SettingsData settingsData;
    public VideoSettings(SettingsData data,bool defaultSettings)
    {
        this.settingsData = data;
        if(defaultSettings)
        {
            SetDefaultSettings();
        }
        SetSettings();
    }



    private void SetSettings()
    {
        SetResolution(new Resolution() { height =  settingsData.resolutionHeight, width = settingsData.resolutionWidth });
        SetFullScreen(settingsData.fullScreen);
        SetFPSLimit(settingsData.fpsLimit);

        SetFont(settingsData.fontIndex);
    }

    #region Set Value
    public void SetDefaultSettings()
    {
        SetDefaultResolution();
        SetDefaultFPSLimit();
        SetFont(0);
        SetFullScreen(true);
    }
    public void SetResolution(Resolution resolution)
    {
        settingsData.resolutionHeight = resolution.height;
        settingsData.resolutionWidth = resolution.width;
        Screen.SetResolution(resolution.width,resolution.height,Screen.fullScreen);
    }
    public void SetFullScreen(bool value)
    {
        settingsData.fullScreen = value;
        Screen.fullScreen = value;
    }
    public void SetFPSLimit(int value)
    {
        settingsData.fpsLimit = value;
        Application.targetFrameRate = value;
        QualitySettings.vSyncCount = 0;
    }
    public void SetFont(int index)
    {
        settingsData.fontIndex = index;
        GamePreferences.instance.NewFont(index);
    }
    #endregion

    #region Set Default
    private void SetDefaultResolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution maxRes = resolutions[0];
        foreach (var res in resolutions)
        {
            if (res.width * res.height > maxRes.width * maxRes.height)
            {
                maxRes = res;
            }
        }
        Debug.Log(resolutions.Length);
        SetResolution(maxRes);
    }
    private void SetDefaultFPSLimit()
    {
        var refreshRate = Screen.currentResolution.refreshRateRatio;
        int fps = (int)((float)refreshRate.numerator / refreshRate.denominator);
        SetFPSLimit(fps);
    }

    #endregion


}

