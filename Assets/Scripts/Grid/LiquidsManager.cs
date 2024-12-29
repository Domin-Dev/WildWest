
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LiquidsManager : MonoBehaviour 
{
    public static LiquidsManager instance { private set; get; }

    public Dictionary<int, WaterHole> waterHoles { private set; get; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        waterHoles = new Dictionary<int, WaterHole>();
    }

    public double GetFill(int waterHoleID)
    {
        if (waterHoles.ContainsKey(waterHoleID))
        {
            return waterHoles[waterHoleID].GetFillTile();
        }
        return -1;
    }
    public void AddNewHoleToWaterHoles(int waterHoleID, GridTile gridTile,float newWater = 0)
    {
        gridTile.GridObjectIsType(out GridHole hole);
        hole.waterHoleID = waterHoleID;
        WaterHole waterHole = waterHoles[waterHoleID];
        waterHole.FillWater(newWater);

        if (!waterHole.IncreaseNumberOfTiles())
        {
            hole.waterLevel = waterHole.GetWaterLevel();
            return;
        }
        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();
        holesToCheck.Add(gridTile);
        int newWaterLevel = waterHole.GetWaterLevel();
        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && !checkedHoles.Contains(tile) && gridHole.waterHoleID == waterHoleID)
                    {
                        gridHole.waterLevel = newWaterLevel;
                        holesToCheck.Add(tile);
                        GridVisualization.instance.UpdateMesh(tile.x, tile.y, true);
                    }
                    checkedHoles.Add(tile);
                }
                holesToCheck.RemoveAt(i);
            }
        }
    }

    public void IncreaseWater(int waterHoleID, GridTile gridTile, float newWater)
    {
        WaterHole waterHole = waterHoles[waterHoleID];
        if (!waterHole.FillWater(newWater))
        {
            return;
        }
        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();
        holesToCheck.Add(gridTile);
        int newWaterLevel = waterHole.GetWaterLevel();
        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && !checkedHoles.Contains(tile) && gridHole.waterHoleID == waterHoleID)
                    {
                        gridHole.waterLevel = newWaterLevel;
                        holesToCheck.Add(tile);
                        GridVisualization.instance.UpdateMesh(tile.x, tile.y, true);
                    }
                    checkedHoles.Add(tile);
                }
                holesToCheck.RemoveAt(i);
            }
        }
    }
    public void NewHole(GridTile gridTile)
    {
        for (int j = 0; j < 4; j++)
        {
            GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(gridTile.GetXYPosition() + MyTools.directions4[j]);
            if (tile != null && tile.GridObjectIsType<GridHole>(out GridHole hole) && hole.waterHoleID != -1)
            {
                AddNewHoleToWaterHoles(hole.waterHoleID, gridTile);
                break;
            }
        }
    }
    private void GetNewWaterHoleID(GridTile gridTile,float water)
    {
        for (int j = 0; j < 4; j++)
        {
            GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(gridTile.GetXYPosition() + MyTools.directions4[j]);
            if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && gridHole.waterHoleID != -1)
            {
                AddNewHoleToWaterHoles(gridHole.waterHoleID, gridTile,water);
                return;
            }
        }

        if (waterHoles.Count > 0)
        {
            int max = waterHoles.Keys.Max();
            for (int j = 0; j < max; j++)
            {
                if (!waterHoles.ContainsKey(j))
                {
                    AddNewWaterHole(j, gridTile, water);
                    return;
                }
            }
        }
        AddNewWaterHole(waterHoles.Count, gridTile, water);
    }

    private void AddNewWaterHole(int waterHoleID,GridTile gridTile,float water)
    {
        WaterHole waterHole = new WaterHole((uint)waterHoleID, water, 1);
        waterHoles.Add(waterHoleID, waterHole);
        gridTile.GridObjectIsType(out GridHole hole);
        hole.waterHoleID = waterHoleID;
        hole.waterLevel = waterHole.GetWaterLevel();
        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

    }
    public void WaterTransfer(GridTile gridTile, float water)
    {
        GridHole gridHole = null;
        gridTile.GridObjectIsType<GridHole>(out gridHole);
        if (gridHole.waterHoleID == -1)
        {
            GetNewWaterHoleID(gridTile, water);
        }
        else
        {
            IncreaseWater(gridHole.waterHoleID, gridTile, water);
        }
    }
}
