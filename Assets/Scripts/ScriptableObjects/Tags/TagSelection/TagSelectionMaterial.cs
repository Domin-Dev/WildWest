using UnityEngine;

[System.Serializable]
public class TagSettingsMaterial: TagSettings
{
    [SerializeField] public Material material;
    [SerializeField] public AudioClip effectSound;
    public TagSettingsMaterial() : base(){}
}

