

using UnityEngine;

public class WaterHole
{
    public uint waterHoleID { private set; get; }
    public double fill{ private set; get; }
    public uint numberOfTiles { private set; get; }


    public WaterHole(uint waterHoleID, double fill, uint numberOfTiles = 1)
    {
        this.waterHoleID = waterHoleID;
        this.fill = fill;
        this.numberOfTiles = numberOfTiles;
    }

    public bool FillWater(float water)
    {
        int oldWaterLevel = GetWaterLevel();
        double newValue = fill + (double)water;
        if (newValue >= 0)
        {
            if (newValue > numberOfTiles * 1000)
                fill = numberOfTiles * 1000;
            else
                fill = newValue;
        }
        else
            fill = 0;
        Debug.Log(oldWaterLevel);
        return oldWaterLevel != GetWaterLevel();
    }
    public double GetFillTile()
    {
        return fill / numberOfTiles;
    }
    public bool IncreaseNumberOfTiles()
    {
        int oldWaterLevel = GetWaterLevel();
        numberOfTiles++;
        return oldWaterLevel != GetWaterLevel();
    }
    public int GetWaterLevel()
    {
        double level = (fill / (float)numberOfTiles) / 1000f;
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

}
