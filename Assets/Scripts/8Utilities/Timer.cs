using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public abstract class Timer
{ 
    protected class TimersUpdater : MonoBehaviour
    {
        public Action action;
        private void Update()
        {
            if (action != null)
                action();
            else
                Destroy(gameObject);
        }
    }
    protected bool timerIsOver = false;
    protected static TimersUpdater updater;

    protected Timer()
    {
        if (updater == null)
        {
            updater = new GameObject("Updater", typeof(TimersUpdater)).GetComponent<TimersUpdater>(); 
            UnityEngine.Object.DontDestroyOnLoad(updater.gameObject);
        }
        updater.action += Update;   
    }  

    public static Delay Create(float delay, Func<bool> func)
    {
        return new Delay(delay, func);  
    }
    public static DelayWithCleanup Create(float delay, Func<bool> func, Action cleanupFunc)
    {
        return new DelayWithCleanup(delay,func,cleanupFunc);
    }
    public static ForwardAndBack Create(Func<bool> forwardFunc, Func<bool> backfunc)
    {
        return new ForwardAndBack(forwardFunc,backfunc);
    }
    public static TimersFor<T> Create<T>(List<(float timer,T value)> values,Func<T,bool> func,bool stopWhenEmpty = false)
    {
        return new TimersFor<T>(values,func,stopWhenEmpty);
    }   
    public static TimersForWithUpdateInterval<T> Create<T>(List<(float timer,T value)> values,Func<T,bool> func,float updateInterval,bool stopWhenEmpty = false)
    {
        return new TimersForWithUpdateInterval<T>(values,func,updateInterval,stopWhenEmpty);
    }
    
    public virtual float GetTime() => 0;
    public void Cancel()
    {
        if(!timerIsOver)
        {
            updater.action -= Update;  
            timerIsOver = true; 
            ExecuteCancel();
        }
    }
    public bool IsEnd()
    {
        return timerIsOver;
    }
    protected abstract void Update();
    protected virtual void ExecuteCancel(){}
}

public class Delay : Timer
{
    private Func<bool> func;
    private float timer;

    public Delay(float timer, Func<bool> func) : base()
    {
        this.func = func;
        this.timer = timer;     
    }
    public override float GetTime()
    {
        return timer;
    }
    protected override void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            if(func())
                Cancel();
        }
    }
} 
public class DelayWithCleanup : Delay
{
    private Action cleanupFunc;
    public DelayWithCleanup(float timer, Func<bool> func, Action cleanupFunc) : base(timer,func)
    {
        this.cleanupFunc = cleanupFunc;
    }
    protected override void ExecuteCancel()
    {
        cleanupFunc?.Invoke();
    }
}
public class ForwardAndBack : Timer
{
    private Func<bool> forwardFunc;
    private Func<bool> backFunc;
    bool isBack = false;

    public ForwardAndBack(Func<bool> forwardFunc, Func<bool> backFunc) : base()
    {
        this.forwardFunc = forwardFunc;
        this.backFunc = backFunc;
    }
    protected override void Update()
    {
        if(!isBack)
        {
            if (forwardFunc())
            {
                isBack = true;
            }
        }
        else
        {
            if(backFunc())
            {
                Cancel();
            }
        }
    }
}
public class TimersFor<T> : Timer
{
    private List<(float timer,T value)> values;
    private Func<T,bool> func;
    private bool stopWhenEmpty;

    public int Count => values.Count;
    public bool IsEmpty => Count == 0;
    public TimersFor(List<(float timer,T value)> values,Func<T,bool> func,bool stopWhenEmpty = false)
    {
        this.values = values;
        this.func = func;
        this.stopWhenEmpty = stopWhenEmpty;
    }
    public void AddValue(float timer,T value)
    {
        values.Add((timer,value));
    }
    public void RemoveValue(T value,bool invokeFunc = true)
    {
        for (int i = values.Count - 1; i >= 0; i--)
        {
            if(EqualityComparer<T>.Default.Equals(value, values[i].value))
            {
                if(invokeFunc)
                    func.Invoke(value);
                values.RemoveAt(i);
            }
        }
    }

    protected override void Update()
    {
        UpdateTimers(Time.deltaTime);
    }
    protected void UpdateTimers(float delta)
    {
        for(int i = values.Count -1; i >= 0;i--)
        {
            float timer = values[i].timer - delta;
            if(timer < 0)
            {
                timer = 0;
                if(func.Invoke(values[i].value))
                {
                    values.RemoveAt(i);
                    if(stopWhenEmpty && IsEmpty)
                        Cancel();
                    continue;
                }
            }
            values[i] = (timer,values[i].value);
        }
    }
}
public class TimersForWithUpdateInterval<T> : TimersFor<T> 
{
    private float updateInterval; 
    private float timer;
    public TimersForWithUpdateInterval(List<(float timer,T value)> values,Func<T,bool> func,float updateInterval,bool stopWhenEmpty = false) : base(values,func,stopWhenEmpty)
    {
        this.updateInterval = updateInterval;
    }
    protected override void Update()
    {
        timer += Time.deltaTime;
        if(timer >= updateInterval)
        {
            timer -= updateInterval;
            UpdateTimers(updateInterval);
        }
    }
}
