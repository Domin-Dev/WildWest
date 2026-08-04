using System.Diagnostics;
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class TagSelection
{
    [OnValueChanged("NewTag")]
    [AllowNesting] 
    public TagBase Tag;
    
    [SerializeReference]
    [ShowIf("isNotNull")]
    [AllowNesting] 
    public TagSettings TagSettings;
    private bool isNotNull => TagSettings != null;
    private void NewTag()
    {
        if(Tag == null)
            TagSettings = null;
        else
        {
            var type = Tag.TagSettings;
            if (type.IsAbstract || type.GetConstructor(System.Type.EmptyTypes) == null)
            {
                TagSettings = null;
                return;
            }
            TagSettings = (TagSettings)System.Activator.CreateInstance(Tag.TagSettings);
        }
    } 
}

[System.Serializable]
public abstract class TagSettings {}

