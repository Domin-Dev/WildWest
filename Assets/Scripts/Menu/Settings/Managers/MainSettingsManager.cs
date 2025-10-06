using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainSettingsManager : MonoBehaviour
{
    public static MainSettingsManager instance { private set; get; }

    public VideoSettings videoSettings { private set; get; }
    public SettingsData settings { private set; get; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        settings = LoadSystem.LoadSettings();
        SetSettings();
    }    


    private void SetSettings()
    {
        bool settingsEmpty = settings == null;
        if (settingsEmpty) settings = new SettingsData();   
        Debug.Log("dzia;!!!");
        videoSettings = new VideoSettings(settings, settingsEmpty);
    }

    public void Save()
    {
        SaveSystem.SaveSettings(settings);
    }

}
