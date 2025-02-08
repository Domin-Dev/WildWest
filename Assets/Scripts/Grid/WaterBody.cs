using System;
using UnityEngine;

public class WaterBody
{
    public int waterBodyID { private set; get; }
    public double fill{ private set; get; }
    public int tileCount { private set; get; }

    public bool hasEmptyNeighbors;


    public WaterBody(int waterBodyID, double fill, int numberOfTiles = 1)
    {
        this.waterBodyID = waterBodyID;
        this.fill = fill;
        this.tileCount = numberOfTiles;
    }


    public bool IncreaseWater(double water)
    {
        int oldWaterLevel = GetWaterLevel();
        double newValue = fill + water;
        if (newValue >= 0)
        {
            if (newValue > tileCount * 1000)
                fill = tileCount * 1000;
            else
                fill = newValue;
        }
        else
            fill = 0;
        return oldWaterLevel != GetWaterLevel();
    }
   
    public bool CollectWater(float water, out float collectedWater, out bool remove)
    {
        if(water <= 0)
        {
            collectedWater = 0;
            remove = false;
            return false;
        }
        int oldWaterLevel = GetWaterLevel();
        double newValue;
        if (fill >= water)
            newValue = fill - water;
        else
            newValue = 0;
        if(newValue == 0)
        {
            water = (float)fill;
            collectedWater = (float)fill;
        }
        else
            collectedWater = water;
        fill = newValue;
        remove =  GetTilesToRemove() > 0;
        return oldWaterLevel != GetWaterLevel();
    }
    public double GetFillTile()
    {
        return fill / tileCount;
    }
    public bool Decrease(double fill = 0,int number = 1)
    {
        int oldWaterLevel = GetWaterLevel();
        tileCount -= number;
        this.fill -= fill;
        return oldWaterLevel != GetWaterLevel();
    }
    public bool Increase(double fill = 0, int number = 1)
    {
        int oldWaterLevel = GetWaterLevel();
        tileCount += number;
        this.fill += fill;
        return oldWaterLevel != GetWaterLevel();
    }
    public bool IncreaseNumberOfTiles(int number = 1)
    {
        int oldWaterLevel = GetWaterLevel();
        tileCount += number;
        return oldWaterLevel != GetWaterLevel();
    }
    public int GetWaterLevel()
    {
        double level = (fill / (float)tileCount) / 1000f;
        Debug.Log(level);
        if (level >= 0.75f)
            return 4;
        else if (level >= 0.5f)
            return 3;
        else if (level >= 0.1f)
            return 2;
        else if (level > 0f)
            return 1;
        else
            return 0;
    }
    public int GetNumberOfFreeTiles()
    {
        return  Mathf.CeilToInt((float)fill / 20f) - tileCount; 
    }

    private int GetTilesToRemove()
    {
        int number = GetNumberOfFreeTiles();
        if (number >= 0) return 0;
        else return -number;
    }
    public bool Check(double newWater = 0)
    {
       return Check(0, 1);
    }
    public bool Check(double newWater = 0, int newTiles = 1)
    {
        return (Math.Ceiling(((float)fill + newWater) / 20f) - (newTiles + tileCount)) >= 0;
    }

}
