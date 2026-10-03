using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Timer
{ 
    protected class TimersUpdater : MonoBehaviour
    {
        public Action action;
        private void Update()
        {
            if (action != null)
            {
                action();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    protected bool timerIsOver = false;
    protected static TimersUpdater updater;

    public static Delay Create(float time, Func<bool> func)
    {
        return new Delay(time, func);  
    }
    public static DelayWithCleanup Create(float timer, Func<bool> func, Action cleanupFunc )
    {
        return new DelayWithCleanup(timer,func,cleanupFunc);
    }
    public static ForwardAndBack Create(Func<bool> forwardFunc, Func<bool> backfunc)
    {
        return new ForwardAndBack(forwardFunc,backfunc);
    }

    protected Timer()
    {
        if (updater == null)
        {
            updater = new GameObject("Updater", typeof(TimersUpdater)).GetComponent<TimersUpdater>();
        }    
        updater.action += Update;   
    }  
    
    public abstract float GetTime();
    protected abstract void Update();
    protected virtual void ExecuteCancel(){}
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
        if(!timerIsOver)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                if(func())
                    Cancel();
            }
        }
    }
} 
public class DelayWithCleanup : Delay
{
    private Action cleanupFunc;
    public DelayWithCleanup(float timer, Func<bool> func, Action cleanupFunc ) : base(timer,func)
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

    public override float GetTime()
    {
        return 0;
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