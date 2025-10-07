using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MainSettingsManager : MonoBehaviour
{
    public static MainSettingsManager instance { private set; get; }

    [SerializeField] private AudioMixer audioMixer;


    public VideoSettings videoSettings { private set; get; }
    public AudioSettings audioSettings { private set; get; }
    public LanguageSettings languageSettings { private set; get; }
    public SettingsData settings { private set; get; }

    private LocaleManager localeManager;

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


        localeManager = this.AddComponent<LocaleManager>();

        videoSettings = new VideoSettings();
        videoSettings.SetUp(settings,settingsEmpty);
        audioSettings = new AudioSettings();
        audioSettings.SetUp(audioMixer,settings,settingsEmpty);
        languageSettings = new LanguageSettings();
        languageSettings.SetUp(localeManager, settings, settingsEmpty);
    }
    public void Save()
    {
        SaveSystem.SaveSettings(settings);
    }
}
