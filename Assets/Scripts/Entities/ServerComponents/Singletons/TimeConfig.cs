
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
public struct TimeConfig: IComponentData
{
    public float DayDuration;
    public float HourDuration;
    public int DayDurationInTicks;
    public int HourDurationInTicks;
    public int SeasonDuration;
}