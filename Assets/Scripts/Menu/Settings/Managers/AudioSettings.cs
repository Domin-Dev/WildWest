using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : Settings
{
    private AudioMixer audioMixer;
    public void SetUp(AudioMixer audioMixer, SettingsData data,bool defaultSettings)
    {
        this.audioMixer = audioMixer;    
        base.SetUp(data, defaultSettings);
    }
    public override void SetSettings()
    {
        SetSoundsVolume(settingsData.soundsVolume);
        SetMusicVolume(settingsData.musicVolume);
    }
    public override void SetDefaultSettings()
    {
        SetSoundsVolume(0.75f);
        SetMusicVolume(0.75f);
    }

    #region Set Value
    public void SetSoundsVolume(float value)
    {
        audioMixer.SetFloat("SoundsVolume", Mathf.Log10(value <= 0 ? 0.0001f : value) * 20f);
        settingsData.soundsVolume = value;
    }
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value <= 0 ? 0.0001f : value) * 20f);
        settingsData.musicVolume = value;
    }

    #endregion
}

