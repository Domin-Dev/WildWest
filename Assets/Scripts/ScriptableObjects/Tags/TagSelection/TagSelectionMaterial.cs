using UnityEngine;

[System.Serializable]
public class TagSettingsMaterial: TagSettings
{
    public Material material;
    public AudioClip effectSound;
    public float influence;
    public TagSettingsMaterial() : base(){}
}

