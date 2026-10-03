using UnityEngine;
using UnityEngine.UI;

public class AudioTab : SettingsTab
{

    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider soundsVolume;
    [SerializeField] private Slider ambientVolume;

    private void OnEnable()
    {
        SetSettings(MainSettingsManager.instance.settings);
    }
    private void OnDisable()
    {
        
    }

    public override void ResetToDefault()
    {
        MainSettingsManager.instance.audioSettings.SetDefaultSettings();
        SetSettings(MainSettingsManager.instance.settings);
    }
    public override void SaveSettings()
    {

    }
    private void SetSettings(SettingsData settings)
    {
        SetMusicVolume(settings);
        SetSoundsVolume(settings);
        SetAmbientVolume(settings);
    }

    private void SetSoundsVolume(SettingsData settingsData)
    {
        soundsVolume.value = settingsData.soundsVolume;
        soundsVolume.onValueChanged.RemoveAllListeners();
        soundsVolume.onValueChanged.AddListener(MainSettingsManager.instance.audioSettings.SetSoundsVolume);
    }
    private void SetMusicVolume(SettingsData settingsData)
    {
        musicVolume.value = settingsData.musicVolume;
        musicVolume.onValueChanged.RemoveAllListeners();
        musicVolume.onValueChanged.AddListener(MainSettingsManager.instance.audioSettings.SetMusicVolume);
    }
    private void SetAmbientVolume(SettingsData settingsData)
    {
        ambientVolume.value = settingsData.ambientVolume;
        ambientVolume.onValueChanged.RemoveAllListeners();
        ambientVolume.onValueChanged.AddListener(MainSettingsManager.instance.audioSettings.SetAmbientVolume);
    }

}


