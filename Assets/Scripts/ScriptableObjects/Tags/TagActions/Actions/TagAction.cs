using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public abstract class TagActionBase : ScriptableObject
{
    public abstract Type TagSettingsType {get;}
    public abstract Type ArgsType {get;}
    public int TagActionID = -1;

    private void OnValidate()
    {
        if (TagActionID == -1) TagActionID = Resources.Load<IDManager>("IDManager").GetNextTagActionID();
    }
    protected (int index,TagActionState state)[] GetActionState(DynamicBuffer<TagActionState> states)
    {
        return GetActionState(states,TagActionID);
    }
    protected (int index,TagActionState state)[] GetActionState(DynamicBuffer<TagActionState> states,int tagActionID)
    {
        List<(int,TagActionState)> tagActions = new ();
        for(int i = 0; i < states.Length;i++)
        {
            var state = states[i];
            if(state.TagActionID == tagActionID)
                tagActions.Add((i,state));
        }
        return tagActions.ToArray();
    }

    protected void RemoveState((int index,TagActionState state)[] states, DynamicBuffer<TagActionState> buffer)
    {
        for(int i = states.Length - 1; i >= 0; i--)
            buffer.RemoveAt(states[i].index);
    }
    protected void RemoveState(DynamicBuffer<TagActionState> buffer,int tagActionID)
    {
        for(int i = buffer.Length - 1; i >= 0 ;i--)
        {
            if( buffer[i].TagActionID == tagActionID)
                buffer.RemoveAt(i);
        }
    }
    protected void RemoveState(DynamicBuffer<TagActionState> buffer)
    {
        RemoveState(buffer,TagActionID);
    }
   
    protected void UpdateState((int,TagActionState)[] states, DynamicBuffer<TagActionState> buffer)
    {
        foreach((int index,var newValue) in states)
            buffer[index] = newValue;
    }
}

public abstract class TagActionBase<T1> : TagActionBase
{
    public abstract void Execute(T1 data,TagSettings tagSettings,TagActionArgsBase args);
}
public abstract class TagActionBase<T1,T2> : TagActionBase
{
    public abstract void Execute(T1 data1,T2 data2,TagSettings tagSettings,TagActionArgsBase args);
}
public abstract class TagActionBase<T1,T2,T3> : TagActionBase
{
    public abstract void Execute(T1 data1,T2 data2,T3 data3,TagSettings tagSettings,TagActionArgsBase args);
}

public abstract class TagAction<T,A,P1> : TagActionBase<P1> where T : TagSettings where A : TagActionArgsBase
{
    public override Type TagSettingsType => typeof(T);
    public override Type ArgsType => typeof(A);
    public override void Execute(P1 data,TagSettings tagSettings,TagActionArgsBase args)
    {
        T t = tagSettings as T;
        A a = args as A;

        if(t != null && a != null)
            Func(data,t,a);
    }
    protected abstract void Func(P1 data,T tagSettings,A args);
}
public abstract class TagAction<T,A,P1,P2> : TagActionBase<P1,P2> where T : TagSettings where A : TagActionArgsBase
{
    public override Type TagSettingsType => typeof(T);
    public override Type ArgsType => typeof(A);
    public override void Execute(P1 data1,P2 data2,TagSettings tagSettings,TagActionArgsBase args)
    {
        T t = tagSettings as T;
        A a = args as A;

        if(t != null && a != null)
            Func(data1,data2,t,a);
    }
    protected abstract void Func(P1 data1,P2 data2,T tagSettings,A args);
}
public abstract class TagAction<T,A,P1,P2,P3> : TagActionBase<P1,P2,P3> where T : TagSettings where A : TagActionArgsBase
{
    public override Type TagSettingsType => typeof(T);
    public override Type ArgsType => typeof(A);
    public override void Execute(P1 data1,P2 data2,P3 data3,TagSettings tagSettings,TagActionArgsBase args)
    {
        T t = tagSettings as T;
        A a = args as A;

        if(t != null && a != null)
            Func(data1,data2,data3,t,a);
    }
    protected abstract void Func(P1 data1,P2 data2,P3 data3,T tagSettings,A args);
}

