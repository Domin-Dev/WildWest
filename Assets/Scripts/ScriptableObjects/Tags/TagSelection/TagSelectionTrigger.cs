using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class TagSettingsTrigger: TagSettings
{
    public TriggerShape triggerShape;

    [Label("Margin (pixels)")]
    [AllowNesting]
    public float margin;
    public TagSettingsTrigger() : base(){}
}
public enum TriggerShape
{
    SpriteShape,
    HitboxShape
}