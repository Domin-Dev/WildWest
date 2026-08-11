using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
using UnityEngine;

public static class TimeService
{
    public static void UpdateCurrentTime(NetworkTick serverTick,RefRW<CurrentTime> currentTime,in TimeConfig timeConfig,out int ticksSince,out bool nextDay,out bool newSeason,out bool nextTimeOfDay)
    {
        ticksSince = serverTick.TicksSince(currentTime.ValueRO.startTick);
        float rawHour = currentTime.ValueRO.startHour + ticksSince / (float)timeConfig.HourDurationInTicks;
        float hour = rawHour;
        newSeason = false;
        nextTimeOfDay = false;
        
             
        if(hour >= 24)
        {
            hour -= 24;
            currentTime.ValueRW.Day++;    
            currentTime.ValueRW.startTick.Add((uint)timeConfig.DayDurationInTicks - (uint)(currentTime.ValueRW.startHour * timeConfig.HourDurationInTicks));
            currentTime.ValueRW.startHour = 0;

            if(currentTime.ValueRO.Day % timeConfig.SeasonDuration == 1)
            {
                currentTime.ValueRW.season = WorldTimeConfig.GetNextSeason(currentTime.ValueRO.season);
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
                currentTime.ValueRO.season,
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
}