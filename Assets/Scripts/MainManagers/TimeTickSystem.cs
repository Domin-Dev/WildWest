using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine.Utility;

public class TimeTickSystem : MonoBehaviour
{
    public class OnTickArgs : EventArgs
    {
        public OnTickArgs(int tick) { this.tick = tick; }
        public int tick;
    }

    public static event EventHandler<OnTickArgs> OnMinuteTick;
    public static event EventHandler<OnTickArgs> OnTick;
    public static event EventHandler<OnTickArgs> On10Tick;
    public static int TicksPerMinute = 600;
    public static float TickTimerMax = 60f/TicksPerMinute;

    private int tick;
    private float tickTimer;

    private void Awake()
    {
        tick = 0;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        if(tickTimer >= TickTimerMax )
        {
            tickTimer = 0;
            tick++;
            OnTick?.Invoke(this,new OnTickArgs(tick));
            if(tick % TicksPerMinute == 0)
            {
                OnMinuteTick?.Invoke(this,new OnTickArgs(tick));
            }
            if(tick % 10 == 0)
                On10Tick?.Invoke(this, new OnTickArgs(tick));
        }
    }
}
