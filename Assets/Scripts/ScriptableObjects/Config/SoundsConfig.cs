using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization;



[CreateAssetMenu(fileName = "SoundsConfig", menuName = "GameAsset/ConfigFiles/SoundsConfig")]
public class SoundsConfig : ScriptableObject
{
    [SerializeField] private List<AudioClip> sounds;
    [SerializeField] public Ambient WindAmbient;
    [SerializeField] public Ambient RainAmbient;
    public List<AudioClip> Sounds => sounds;
}
[System.Serializable]
public class Ambient
{
    public string AmbientName;
    public AudioMixerGroup AudioMixer;
    public AmbientClip[] AmbientClips;

    public bool TryGetAmbientClip(float value,out AmbientClip ambientClip)
    {
        ambientClip = null;
        float currentThreshold = float.MinValue;
        foreach(var ambient in AmbientClips)
        {
            if(ambient.ActivationRangeMin < value && ambient.ActivationRangeMax > value && currentThreshold < ambient.ActivationRangeMin)
            {
                currentThreshold = ambient.ActivationRangeMin;
                ambientClip = ambient;
            }
        }
        return ambientClip != null;
    }
}

[System.Serializable]
public class AmbientClip
{
    public AudioClip AudioClip;
    public float ActivationRangeMin;
    public float ActivationRangeMax;

    [Space]
    public PitchMode PitchMode;
    public float PitchMin;
    public float PitchMax;
}
public enum PitchMode
{
    Random,
    DependingOnValue
}