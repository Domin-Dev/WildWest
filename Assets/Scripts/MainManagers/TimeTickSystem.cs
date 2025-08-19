using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine.Utility;

public class TimeTickSystem : MonoBehaviour
{
    public class OnTickArgs : EventArgs
    {
        public OnTickArgs(int tick) { 
            this.tick = tick; 
        }
        public int tick;
        public int totalTime;
    }

    public static event EventHandler<OnTickArgs> OnMinuteTick;
    public static event EventHandler<OnTickArgs> On10SecondsTick;


    public static event EventHandler<OnTickArgs> OnTick;
    public static event EventHandler<OnTickArgs> On10Tick;
    public static int TicksPerMinute = 600;
    public static float TickTimerMax = 60f/TicksPerMinute;
    public static int TickPer10Seconds = TicksPerMinute/6;

    private int tick;
    private float tickTimer;

    private double totalTime;
    private void Awake()
    {
        tick = 0;
        totalTime = GameInfo.instance.playTime;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        totalTime += Time.deltaTime;

        if (tickTimer >= TickTimerMax )
        {
            tickTimer = tickTimer - TickTimerMax;
            tick++;
            OnTick?.Invoke(this,new OnTickArgs(tick));
            if(tick % TicksPerMinute == 0)
            {
                OnMinuteTick?.Invoke(this,new OnTickArgs(tick));
            }

            if (tick % 10 == 0)
            {
                On10Tick?.Invoke(this, new OnTickArgs(tick));
                GameInfo.instance.playTime = totalTime;
            }
          
        }
    }
}
