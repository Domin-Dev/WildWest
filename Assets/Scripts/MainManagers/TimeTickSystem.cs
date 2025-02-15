using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.ConstrainedExecution;

public class TimeTickSystem : MonoBehaviour
{
    public class OnTickArgs : EventArgs
    {
        public OnTickArgs(int tick) { this.tick = tick; }
        public int tick;
    }

    public static event EventHandler<OnTickArgs> OnMinuteTick;
    public static event EventHandler<OnTickArgs> OnTick;
    private const float TickTimerMax = 0.1f;
    private const int TicksPerMinute = (int)(60f/TickTimerMax);

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
        }
    }
}
