using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;



[CreateAssetMenu(fileName = "SoundsConfig", menuName = "GameAsset/ConfigFiles/SoundsConfig")]
public class SoundsConfig : ScriptableObject
{
    [SerializeField] private List<AudioClip> sounds;
    public List<AudioClip> Sounds => sounds;
}
