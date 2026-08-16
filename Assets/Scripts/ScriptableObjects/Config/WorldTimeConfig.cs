using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using Unity.Entities.UniversalDelegates;
using JetBrains.Annotations;

[CreateAssetMenu(fileName = "WorldTimeConfig", menuName = "GameAsset/ConfigFiles/WorldTimeConfig")]
public class WorldTimeConfig : ScriptableObject
{
    [Header("Start Settings")]
    [Range(0,24)]
    public int startHour;
    [Min(1)]
    public int startDay;
    public Season startSeason;
    [Header("Day Settings")]
    [Label("Hour duration (minutes)")]
    [Min(0.001f)]
    public float HourDuration;
    [Label("Season duration (days)")]
    [Min(1)]
    public int SeasonDuration;
    public int YearDuration => SeasonDuration * 4;
    [Header("Season Settings")]
    public SeasonConfig Spring;
    public SeasonConfig Summer;
    public SeasonConfig Autumn;
    public SeasonConfig Winter;

    [Header("Time of Day UI")]
    public TimeOfDayUI[] rangesUI;


    public (Color color,float dayLerpTime) GetTimeOfDayColor(in CurrentTime currentTime)
    {
        var season = GetSeason(currentTime.season);
        return season.GetColor(currentTime.TimeOfDay);
    }
    public (Color color,float dayLerpTime) GetPreviousTimeOfDayColor(in CurrentTime currentTime)
    {
        Season season = currentTime.season;
        if(currentTime.TimeOfDay == TimeOfDay.Day)
        {
            int seasonDay = (currentTime.Day - 1) % SeasonDuration;
            if(seasonDay == 0)
                season = GetPreviousSeason(season);
        }

        var config = GetSeason(season);
        return config.GetColor(GetPreviousTimeOfDay(currentTime.TimeOfDay));
    }
    public SeasonConfig[] Seasons => new SeasonConfig[]
    {
        Spring, Summer, Autumn, Winter  
    };
    public SeasonConfig GetSeason(Season season)
    {
        switch(season)
        {
            case Season.Spring:
                return Spring;
            case Season.Summer:
                return Summer;
            case Season.Autumn:
                return Autumn;
            case Season.Winter:
                return Winter;
        }
        return null;
    }
    public double GetWorldTimeStartCurrentSeason(in CurrentTime currentTime)
    {
        int x = (currentTime.Day -1) % SeasonDuration; 
        return currentTime.GetWorldTime(0,currentTime.Day - x);
    }
    public static Season GetNextSeason(Season current)
    {
        return (Season)(((int)current + 1) % 4); 
    } 
    public static Season GetPreviousSeason(Season current)
    {
        return (Season)(((int)current + 3) % 4);
    }     
    public static TimeOfDay GetPreviousTimeOfDay(TimeOfDay current)
    {
        return (TimeOfDay)(((int)current + 2) % 3);
    } 
    public DailySchedule GetDailySchedule(Season current,int currentDay)
    {
        int seasonDay =   (currentDay - 1) % SeasonDuration;
        float t = seasonDay / (float)(SeasonDuration - 1);
        Season targetSeason;
        Season startSeason;
         
        if(t < 0.5f)
        {
            t += 0.5f;
            targetSeason = current;
            startSeason = GetPreviousSeason(current);
        }
        else
        {
            t -= 0.5f;
            targetSeason = GetNextSeason(current);
            startSeason = current;
        }

        SeasonConfig targetConfig = GetSeason(targetSeason);
        SeasonConfig startConfig = GetSeason(startSeason);
        return new DailySchedule()
        {
            dayStart = Mathf.Lerp(startConfig.dayTime.dayStart,targetConfig.dayTime.dayStart,t),
            eveningStart = Mathf.Lerp(startConfig.dayTime.eveningStart,targetConfig.dayTime.eveningStart,t),
            nightStart = Mathf.Lerp(startConfig.dayTime.nightStart,targetConfig.dayTime.nightStart,t),
        };
    }
    public float GetTimeOfDayThreshold(Season season,float hour,int day,out TimeOfDay currentTimeOfDay)
    {
        var dailySchedule = GetDailySchedule(season,day);
        if(hour < dailySchedule.dayStart)
        {
            currentTimeOfDay = TimeOfDay.Night;
            return dailySchedule.dayStart;
        }
        else if(hour < dailySchedule.eveningStart)
        {
            currentTimeOfDay = TimeOfDay.Day;
            return dailySchedule.eveningStart;
        }
        else if(hour < dailySchedule.nightStart)
        {
            currentTimeOfDay = TimeOfDay.Evening;
            return dailySchedule.nightStart;
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Night;
            return 24;
        } 
    }
}



[System.Serializable]
public class TimeOfDayUI : RangeUI
{
    public TimeOfDay TimeOfDay;
} 
public class RangeUI
{
    public Sprite iconSprite;
    public Sprite barSprite;
} 



[System.Serializable]
public class SeasonConfig : RangeUI
{
    public DailySchedule dayTime;
    public WhiteBalance whiteBalance;

    [Header("Times of day")]
    
    public Color dayColor;
    [Min(0.1f)]
    [Label("Day lerp duration (game hour)")]
    public float dayLerpDuration;
   
    public Color eveningColor;
    [Min(0.1f)]
    [Label("Evening lerp duration (game hour)")]
    public float eveningLerpDuration;
    
    public Color nightColor;
    [Min(0.1f)]
    [Label("Night lerp duration (game hour)")]
    public float nightLerpDuration;

    public (Color color,float lerpTIme) GetColor(TimeOfDay timeOfDay)
    {
        switch(timeOfDay)
        {
            case TimeOfDay.Day:
                return (dayColor,dayLerpDuration);
            case TimeOfDay.Evening:
                return (eveningColor,eveningLerpDuration);
            case TimeOfDay.Night:
                return (nightColor,nightLerpDuration);
        }
        return default;
    }
}
public enum Season : byte
{
    Spring = 0,
    Summer = 1, 
    Autumn = 2, 
    Winter = 3  
}
public enum  TimeOfDay : byte
{
    Day = 0,
    Evening = 1,
    Night = 2
}

[System.Serializable]
public struct WhiteBalance
{
    [Range(-100f, 100f)]
    public float Temperature;
    [Range(-100f, 100f)]
    public float Tint;
    [Min(0.1f)]
    [Label("Lerp duration (game hour)")]
    public float LerpDuration;
}

[System.Serializable]
public struct DailySchedule
{    
    [Range(0, 24)]
    public float dayStart;
    [Range(0, 24)]
    public float eveningStart;
    [Range(0, 24)]
    public float nightStart;
    public float[] GetTimes()
    {
        float[] times = new float[4];
        times[0] = dayStart;
        times[1] = eveningStart - dayStart;
        times[2] = nightStart - eveningStart;
        times[3] = 24 - nightStart;
        return times;
    }

    public float GetStartTimeOfDay(TimeOfDay timeOfDay)
    {
        switch(timeOfDay)
        {
            case TimeOfDay.Day:
                return dayStart;
            case TimeOfDay.Evening:
                return eveningStart;
            case TimeOfDay.Night:
                return nightStart;
        }
        return default;
    }
}