using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NaughtyAttributes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class TagEvent<T>
{
    public TagActionAssignment<T>[] Actions;
    public ReadyAction<T>[] GetActions(Item item)
    {
        List<ReadyAction<T>> list = new ();
        foreach(var action in Actions)
        {
            var tag = item.tags.FirstOrDefault(x => x.Tag.ID == action.Tag.ID);
            if(tag != null)
                list.Add(new ReadyAction<T>(action,tag.TagSettings));         
        }
        return list.ToArray();
    }
}

[System.Serializable]
public class TagEvent<T1,T2>
{
    public TagActionAssignment<T1,T2>[] Actions;
    public ReadyAction<T1,T2>[] GetActions(Item item)
    {
        List<ReadyAction<T1,T2>> list = new ();
        foreach(var action in Actions)
        {
            var tag = item.tags.FirstOrDefault(x => x.Tag.ID == action.Tag.ID);
            if(tag != null)
                list.Add(new ReadyAction<T1,T2>(action,tag.TagSettings));         
        }
        return list.ToArray();
    }
}

[System.Serializable]
public class TagEvent<T1,T2,T3>
{
    public TagActionAssignment<T1,T2,T3>[] Actions;
    public ReadyAction<T1,T2,T3>[] GetActions(Item item)
    {
        List<ReadyAction<T1,T2,T3>> list = new ();
        foreach(var action in Actions)
        {
            var tag = item.tags.FirstOrDefault(x => x.Tag.ID == action.Tag.ID);
            if(tag != null)
                list.Add(new ReadyAction<T1,T2,T3>(action,tag.TagSettings));         
        }
        return list.ToArray();
    }
}


[System.Serializable]
public class TagActionAssignment<T>
{
    [OnValueChanged("NewAction")]
    [AllowNesting] 
    public TagBase Tag;

    [SerializeReference]
    [ShowIf("isNotNull")]
    [OnValueChanged("NewAction")]
    [AllowNesting] 
    public TagActionBase<T> tagActionBase;

    [SerializeReference]
    [ShowIf("showArgs")]
    [AllowNesting] 
    public TagActionArgsBase tagActionArgs;
    

    private bool isNotNull => Tag != null;
    private bool showArgs => isNotNull && tagActionArgs != null;
    private void NewAction()
    {
        if(Tag != null && tagActionBase != null)
        {
            if(Tag.TagSettings.GetType() != tagActionBase.TagSettingsType.GetType())
                tagActionBase = null;
            else
            {
                var type = tagActionBase.ArgsType;
                if (type.IsAbstract || type.GetConstructor(System.Type.EmptyTypes) == null)
                {
                    tagActionArgs = null;
                    return;
                }
                tagActionArgs = (TagActionArgsBase)System.Activator.CreateInstance(type);
            }
        }
           
    }
}

[System.Serializable]
public class TagActionAssignment<T1,T2>
{
    [OnValueChanged("NewAction")]
    [AllowNesting] 
    public TagBase Tag;

    [SerializeReference]
    [ShowIf("isNotNull")]
    [OnValueChanged("NewAction")]
    [AllowNesting] 
    public TagActionBase<T1,T2> tagActionBase;

    [SerializeReference]
    [ShowIf("showArgs")]
    [AllowNesting] 
    public TagActionArgsBase tagActionArgs;
    

    private bool isNotNull => Tag != null;
    private bool showArgs => isNotNull && tagActionArgs != null;
    private void NewAction()
    {
        if(Tag != null && tagActionBase != null)
        {
            if(Tag.TagSettings.GetType() != tagActionBase.TagSettingsType.GetType())
                tagActionBase = null;
            else
            {
                var type = tagActionBase.ArgsType;
                if (type.IsAbstract || type.GetConstructor(System.Type.EmptyTypes) == null)
                {
                    tagActionArgs = null;
                    return;
                }
                tagActionArgs = (TagActionArgsBase)System.Activator.CreateInstance(type);
            }
        }
           
    }
}

[System.Serializable]
public class TagActionAssignment<T1,T2,T3>
{
    [OnValueChanged("NewAction")]
    [AllowNesting] 
    public TagBase Tag;

    [SerializeReference]
    [ShowIf("isNotNull")]
    [OnValueChanged("NewAction")]
    [AllowNesting] 
    public TagActionBase<T1,T2,T3> tagActionBase;

    [SerializeReference]
    [ShowIf("showArgs")]
    [AllowNesting] 
    public TagActionArgsBase tagActionArgs;
    

    private bool isNotNull => Tag != null;
    private bool showArgs => isNotNull && tagActionArgs != null;
    private void NewAction()
    {
        if(Tag != null && tagActionBase != null)
        {
            if(Tag.TagSettings.GetType() != tagActionBase.TagSettingsType.GetType())
                tagActionBase = null;
            else
            {
                var type = tagActionBase.ArgsType;
                if (type.IsAbstract || type.GetConstructor(System.Type.EmptyTypes) == null)
                {
                    tagActionArgs = null;
                    return;
                }
                tagActionArgs = (TagActionArgsBase)System.Activator.CreateInstance(type);
            }
        }
           
    }
}





[System.Serializable]
public class ReadyAction<T>
{
    public TagActionAssignment<T> tagActionAssignment;
    public TagSettings tagSettings;
    public ReadyAction(TagActionAssignment<T> tagActionAssignment,TagSettings tagSettings)
    {
        this.tagActionAssignment = tagActionAssignment;
        this.tagSettings = tagSettings;
    }
    public void Run(T context)
    {
        tagActionAssignment.tagActionBase.Execute(context,tagSettings,tagActionAssignment.tagActionArgs);
    }
}
[System.Serializable]
public class ReadyAction<T1,T2>
{
    public TagActionAssignment<T1,T2> tagActionAssignment;
    public TagSettings tagSettings;
    public ReadyAction(TagActionAssignment<T1,T2> tagActionAssignment,TagSettings tagSettings)
    {
        this.tagActionAssignment = tagActionAssignment;
        this.tagSettings = tagSettings;
    }
    public void Run(T1 context1,T2 context2)
    {
        tagActionAssignment.tagActionBase.Execute(context1,context2,tagSettings,tagActionAssignment.tagActionArgs);
    }
}
public class ReadyAction<T1,T2,T3>
{
    public TagActionAssignment<T1,T2,T3> tagActionAssignment;
    public TagSettings tagSettings;
    public ReadyAction(TagActionAssignment<T1,T2,T3> tagActionAssignment,TagSettings tagSettings)
    {
        this.tagActionAssignment = tagActionAssignment;
        this.tagSettings = tagSettings;
    }
    public void Run(T1 context1,T2 context2, T3 context3)
    {
        tagActionAssignment.tagActionBase.Execute(context1,context2,context3,tagSettings,tagActionAssignment.tagActionArgs);
    }
}



[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct TagValue
{
    [FieldOffset(0)]
    public int Int;

    [FieldOffset(0)]
    public float Float;

    [FieldOffset(0)]
    public bool Bool;


}
public enum TagValueType : byte
{
    Int,
    Float,
    Entity,
    Bool,
}
public struct TagActionState : IBufferElementData
{
    public int TagActionID;
    public TagValueType Type;
    public TagValue Value;
}