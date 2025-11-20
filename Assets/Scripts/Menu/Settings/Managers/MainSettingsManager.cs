using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MainSettingsManager : MonoBehaviour
{
    public static MainSettingsManager instance { private set; get; }

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private InputActionAsset inputActions;

    public VideoSettings videoSettings { private set; get; }
    public AudioSettings audioSettings { private set; get; }
    public LanguageSettings languageSettings { private set; get; }
    [SerializeField] public SettingsData settings;
    public ControlsSettings controlsSettings { private set; get; }

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
        SetSettings();
    }


    private void OnEnable()
    {
        
    }
    private void OnDestroy()
    {
        
        Save();
    }

    private void SetSettings()
    {
        Load();
        bool settingsEmpty = settings == null;
        if (settingsEmpty) settings = new SettingsData();
  

        Debug.Log(settingsEmpty);

        localeManager = this.AddComponent<LocaleManager>();

        videoSettings = new VideoSettings();
        videoSettings.SetUp(settings,settingsEmpty);
        audioSettings = new AudioSettings();
        audioSettings.SetUp(audioMixer,settings,settingsEmpty);
        languageSettings = new LanguageSettings();
        languageSettings.SetUp(localeManager, settings, settingsEmpty);

        controlsSettings = new ControlsSettings();
        controlsSettings.SetUp(inputActions, settings, settingsEmpty);
        var input = this.AddComponent<InputManager>();
        input.SetUp(inputActions);
    }

    private void Load()
    {
        LoadSystem.LoadSettings(out string controlJson,out SettingsData settingsData);
        settings = settingsData;
        inputActions.Disable();
        inputActions.LoadBindingOverridesFromJson(controlJson);
        inputActions.Enable();
    }

    private void Save()
    {
        SaveSystem.SaveSettings(settings,inputActions);
    }
}
