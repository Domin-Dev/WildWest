using System;
using Unity.VisualScripting;
using UnityEngine;

public class Timer
{ 
    private class TimersUpdater : MonoBehaviour
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

    private Func<bool> func;
    private Func<bool> backFunc;

    bool isBack = false;

    private float timer;
    private bool timerIsOver = false;


    public static Timer Create(float time, Func<bool> func)
    {
        Timer timer = new Timer(time, func);
        return timer;         
    }
    public static Timer Create(Func<bool> func, Func<bool> backfunc)
    {
        Timer timer = new Timer(func,backfunc);
        return timer;
    }

    private static TimersUpdater updater;
    public Timer(float timer, Func<bool> func)
    {
        this.func = func;
        this.timer = timer;
        TimersUpdater timersUpdater;
        if (updater == null)
        {
            timersUpdater = new GameObject("Updater", typeof(TimersUpdater)).GetComponent<TimersUpdater>();
            updater = timersUpdater;
        }
        else
        {
            timersUpdater = updater;
        }
        timersUpdater.action += Update;        
    }
    public Timer(Func<bool> func, Func<bool> backfunc)
    {
        this.func = func;
        this.backFunc = backfunc;
        TimersUpdater timersUpdater;
        if (updater == null)
        {
            timersUpdater = new GameObject("Updater", typeof(TimersUpdater)).GetComponent<TimersUpdater>();
            updater = timersUpdater;
        }
        else
        {
            timersUpdater = updater;
        }
        timersUpdater.action += UpdateFunc;
    }

    public void UpdateFunc()
    {
        if(!isBack)
        {
            if (func())
            {
                isBack = true;
            }
        }
        else
        {
            if(backFunc())
            {
                updater.action -= UpdateFunc;
                timerIsOver = true;
            }
        }
    }
    public void Update()
    {
        if(!timerIsOver)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timerIsOver = true;
                func();
                updater.action -= Update;
            }
        }
    }

    public void Cancel()
    {
        if(!timerIsOver)
        {
            timerIsOver = true;
            if (backFunc != null)
            {
                updater.action -= UpdateFunc;
            }
            else
            {
                updater.action -= Update;
            }
        }
    }
    public float GetTime()
    {
        return timer;
    }
    public bool IsEnd()
    {
        return timerIsOver;
    }
}
