using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
using UnityEngine;

public static class TimeService
{
    public static void UpdateCurrentTime(NetworkTick serverTick,RefRW<CurrentTime> currentTime,in TimeConfig timeConfig,out int ticksSince,out bool nextDay,out bool newSeason,out bool nextTimeOfDay)
    {
        ticksSince = serverTick.TicksSince(currentTime.ValueRO.StartTick);
        float rawHour = currentTime.ValueRO.StartHour + ticksSince / (float)timeConfig.HourDurationInTicks;
        float hour = rawHour;
        newSeason = false;
        nextTimeOfDay = false;
               
        if(hour >= 24)
        {
            hour -= 24;
            currentTime.ValueRW.Day++;    
            currentTime.ValueRW.StartTick.Add(timeConfig.DayDurationInTicks - (uint)(currentTime.ValueRW.StartHour * timeConfig.HourDurationInTicks));
            currentTime.ValueRW.StartHour = 0;

            if(currentTime.ValueRO.Day % timeConfig.SeasonDuration == 1)
            {
                currentTime.ValueRW.Season = WorldTimeConfig.GetNextSeason(currentTime.ValueRO.Season);
                newSeason = true;
            }
            nextDay = true;
        }
        else
        {
            nextDay = false;
        }
        
        if(rawHour >= currentTime.ValueRO.NextTimeOfDay)
        {
            currentTime.ValueRW.NextTimeOfDay = WorldConfig.TimeConfig.GetTimeOfDayThreshold(
                currentTime.ValueRO.Season,
                hour,
                currentTime.ValueRO.Day,
                out TimeOfDay currentTimeOfDay);
            if(currentTimeOfDay != currentTime.ValueRO.TimeOfDay)
            {
                nextTimeOfDay = true;
                currentTime.ValueRW.TimeOfDay = currentTimeOfDay;
            }
        }
        currentTime.ValueRW.Hour = hour;
    }
    public static void SetCurrentTime(NetworkTick currentTick,RefRW<CurrentTime> currentTime,double newWorldTime,in TimeConfig timeConfig)
    {
        double currentWorldTime = currentTime.ValueRO.WorldTime;
        double offset = newWorldTime - currentWorldTime;
        if(offset <= 0 || !currentTime.ValueRO.StartTick.IsValid)
            return; 

        int days = 1 + (int)newWorldTime / 24;
        int dayOffset = days - currentTime.ValueRO.Day;
        if(dayOffset > 0)
            currentTime.ValueRW.Day = days;

        currentTime.ValueRW.StartTick = currentTick;
        currentTime.ValueRW.Hour = (float)(newWorldTime % 24);
        currentTime.ValueRW.StartHour = currentTime.ValueRO.Hour;

        currentTime.ValueRW.NextTimeOfDay = WorldConfig.TimeConfig.GetTimeOfDayThreshold(currentTime.ValueRO.Season,  currentTime.ValueRO.Hour , days, out TimeOfDay currentTimeOfDay);
        currentTime.ValueRW.TimeOfDay = currentTimeOfDay;
        currentTime.ValueRW.Season = WorldConfig.TimeConfig.GetCurrentSeason(currentTime.ValueRW.WorldTime);
    }
    public static double GetWorldTime(this CurrentTime currentTime,float hour)
    {
        return GetWorldTime(hour,currentTime.Day);
    }
    public static double GetWorldTime(float hour,int day)
    {
        return hour + (day-1) * 24f;
    } 
}