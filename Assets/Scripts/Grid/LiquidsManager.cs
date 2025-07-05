
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LiquidsManager : MonoBehaviour 
{
    public static LiquidsManager instance { private set; get; }

    public Dictionary<int, WaterBody> waterBodies { private set; get; }
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
        waterBodies = new Dictionary<int, WaterBody>();
    }

    public double GetFill(int waterHoleID)
    {
        if (waterBodies.ContainsKey(waterHoleID))
        {
            return waterBodies[waterHoleID].GetFillTile();
        }
        return -1;
    }
    public bool AddNewHoleToWaterHoles(int waterHoleID, GridTile gridTile,float newWater = 0)
    {
        WaterBody waterHole = waterBodies[waterHoleID];

        if (!waterHole.Check(newWater))
        {
            return false;
        }

        gridTile.GridObjectIsType(out GridHole hole);
        hole.waterHoleID = waterHoleID;
        hole.waterLevel = waterHole.GetWaterLevel();
        waterHole.IncreaseWater(newWater);
        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);
        if (!waterHole.IncreaseNumberOfTiles() && !waterHole.hasEmptyNeighbors) return true;

        hole.waterLevel = waterHole.GetWaterLevel();
        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

        UpdateWaterHole(waterHole, gridTile);
        if(waterHole.hasEmptyNeighbors) WaterSpill(waterHole, gridTile);
        return true;
    }
    public int GetNumberTiles(int waterHoleID)
    {
        if (waterBodies.ContainsKey(waterHoleID))
            return (int)waterBodies[waterHoleID].tileCount;
        else
            return 0;
    }
    private void UpdateWaterHole(WaterBody waterHole, GridTile gridTile)
    {
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
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && !checkedHoles.Contains(tile) && gridHole.waterHoleID == waterHole.waterBodyID)
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
    private void IncreaseWater(int waterBodyID, GridTile gridTile, float newWater)
    {
        WaterBody waterHole = waterBodies[waterBodyID];
        if (!waterHole.IncreaseWater(newWater) && !waterHole.hasEmptyNeighbors) return;
        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();
        holesToCheck.Add(gridTile);
        int newWaterLevel = waterHole.GetWaterLevel();
        gridTile.GridObjectIsType(out GridHole gridHole);
        gridHole.waterLevel = newWaterLevel;
        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

        if(waterHole.hasEmptyNeighbors)
        {
            WaterSpill(waterHole, gridTile);
            return;
        }

        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out gridHole) && !checkedHoles.Contains(tile) && gridHole.waterHoleID == waterBodyID)
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
    public float DecreaseWater(int waterBodyID, GridTile gridTile, float value)
    {
        float collectedWater;
        bool remove;
        WaterBody waterBody = waterBodies[waterBodyID];
        if (waterBody.CollectWater(value, out collectedWater,out remove))
        {
            int newWaterLevel = waterBody.GetWaterLevel();
            if (newWaterLevel == 0 )
            {
                RemoveEmptyHoles(gridTile);
                waterBodies.Remove(waterBodyID); 
            }
            else
            {
                UpdateWaterHole(waterBody, gridTile);
            }
        }
        return collectedWater;
    }
    private void RemoveEmptyHoles(GridTile gridTile)
    {
        gridTile.GridObjectIsType(out GridHole hole);
        int oldID = hole.waterHoleID;
        hole.waterHoleID = -1;
        hole.waterLevel = 0;

        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();
        holesToCheck.Add(gridTile);

        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && !checkedHoles.Contains(tile) && gridHole.waterHoleID == oldID)
                    {
                         gridHole.waterLevel = 0;
                        gridHole.waterHoleID = -1;
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
        List<int> bodies = new List<int>();
        for (int j = 0; j < 4; j++)
        {
            GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(gridTile.GetXYPosition() + MyTools.directions4[j]);
            if (tile != null && tile.GridObjectIsType<GridHole>(out GridHole hole) && hole.waterHoleID != -1)
            {
                waterBodies[hole.waterHoleID].hasEmptyNeighbors = true;
                if(!bodies.Contains(hole.waterHoleID)) bodies.Add(hole.waterHoleID);   
            }
        }
        if (bodies.Count == 0) return;


        if(bodies.Count > 1)
            CompileWaterBodies(bodies, gridTile);
        else
            AddNewHoleToWaterHoles(bodies[0], gridTile);
    }
    public void RemoveHole(GridTile gridTile)
    {
        if(gridTile != null && gridTile.GridObjectIsType(out GridHole hole) && hole.waterHoleID >= 0)
        {
            WaterBody waterBody = waterBodies[hole.waterHoleID];
            if (waterBody.Decrease(waterBody.GetFillTile(),1))
            {
                UpdateWaterHole(waterBody,gridTile);
            }

            for (int i = 0; i < 4; i++)
            {
                GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(gridTile.GetXYPosition() + MyTools.directions4[i]);
                if(tile != null) CheckWaterBody(tile, gridTile);
            }
        }
    }
    private void CompileWaterBody(List<int> bodies, GridTile gridTile, WaterBody newWaterBody)
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            WaterBody body = waterBodies[bodies[i]];

            if (newWaterBody.Check(body.fill, body.tileCount))
            {
                newWaterBody.IncreaseNumberOfTiles(body.tileCount);
                newWaterBody.IncreaseWater(body.fill);
            }
            else
            {
                bodies.RemoveRange(i, bodies.Count - i);
                ChangeWaterBodyID(bodies, gridTile, newWaterBody);
                break;
            }
        }
        ChangeWaterBodyID(bodies, gridTile, newWaterBody);
    }
    private void CompileWaterBodies(List<int> bodies, GridTile gridTile)
    {
        int mainWaterBody = -1;
        for (int i = 0; i < bodies.Count; i++)
        {
            int id = bodies[i];
            if (waterBodies[id].Check(0))
            {
                AddNewHoleToWaterHoles(id, gridTile);
                mainWaterBody = id;
                break;
            } 
        }
        if (mainWaterBody == -1) return;
        bodies.Remove(mainWaterBody);
        WaterBody mainbody = waterBodies[mainWaterBody];    

        for (int i = 0; i < bodies.Count; i++)
        {
            if (!waterBodies.ContainsKey(bodies[i])) continue;
            WaterBody body = waterBodies[bodies[i]];

            if (mainbody.Check(body.fill, body.tileCount))
            {
                mainbody.IncreaseNumberOfTiles(body.tileCount);
                mainbody.IncreaseWater(body.fill);
            }
            else
            {
                bodies.RemoveRange(i, bodies.Count - i);
                ChangeWaterBodyID(bodies, gridTile, mainbody);
                break;
            }
        }
        ChangeWaterBodyID(bodies, gridTile, mainbody);
    }
    private void ChangeWaterBodyID(List<int> bodies, GridTile gridTile, WaterBody newWaterBody)
    {
        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();
        holesToCheck.Add(gridTile);
        int newWaterLevel = newWaterBody.GetWaterLevel();

        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && !checkedHoles.Contains(tile) 
                    && (bodies.Contains(gridHole.waterHoleID) || gridHole.waterHoleID == newWaterBody.waterBodyID))
                    {
                        gridHole.waterLevel = newWaterLevel;
                        gridHole.waterHoleID = newWaterBody.waterBodyID;
                        holesToCheck.Add(tile);
                        GridVisualization.instance.UpdateMesh(tile.x, tile.y, true);
                    }
                    checkedHoles.Add(tile);
                }
                holesToCheck.RemoveAt(i);
            }
        }
        for (int i = 0; i < bodies.Count; i++)
        {
            waterBodies.Remove(bodies[i]);
        }
    }
    private void GetNewWaterHoleID(GridTile gridTile,float water)
    {
        for (int j = 0; j < 4; j++)
        {
            GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(gridTile.GetXYPosition() + MyTools.directions4[j]);
            if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && gridHole.waterHoleID != -1)
            {
               if (AddNewHoleToWaterHoles(gridHole.waterHoleID, gridTile,water)) return;
            }
        }

        AddNewWaterHole(GetNewID(), gridTile, water);
    }
    private int GetNewID()
    {
        if (waterBodies.Count > 0)
        {
            int max = waterBodies.Keys.Max();
            for (int j = 0; j < max; j++)
            {
                if (!waterBodies.ContainsKey(j))
                    return j;           
                else if (waterBodies[j].tileCount == 0) 
                {
                    waterBodies.Remove(j);
                    return j;
                }
            }
        }
        return waterBodies.Count;
    }
    private void AddNewWaterHole(int waterHoleID,GridTile gridTile,float water)
    {
        WaterBody waterHole = new WaterBody(waterHoleID, water, 1);
        waterBodies.Add(waterHoleID, waterHole);
        gridTile.GridObjectIsType(out GridHole hole);
        hole.waterHoleID = waterHoleID;
        hole.waterLevel = waterHole.GetWaterLevel();
        GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

        WaterSpill(waterHole, gridTile);
    }
    public void WaterTransfer(GridTile gridTile, float water)
    {
        GridHole gridHole = null;
        gridTile.GridObjectIsType<GridHole>(out gridHole);
        if (gridHole == null) return;

        if (gridHole.waterHoleID == -1)
        {
            GetNewWaterHoleID(gridTile, water);
        }
        else
        {
            IncreaseWater(gridHole.waterHoleID, gridTile, water);
        }
    }
    private void WaterSpill(WaterBody waterHole,GridTile gridTile)
    {
        gridTile.GridObjectIsType(out GridHole hole);
        int maxNumber = waterHole.GetNumberOfFreeTiles();
        if (maxNumber == 0) return;

        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();

        List<GridTile> selectedHoles = new List<GridTile>();
        List<int> newWaterbodies = new List<int>();

        holesToCheck.Add(gridTile);

        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) &&  !checkedHoles.Contains(tile))
                    {
                        if(gridHole.waterHoleID == -1)
                        {
                            if (selectedHoles.Count == maxNumber)
                            {
                                waterHole.hasEmptyNeighbors = true;
                                goto setValues;
                            }
                            selectedHoles.Add(tile);
                        }
                        else if(gridHole.waterHoleID != waterHole.waterBodyID)
                        {
                            if (!newWaterbodies.Contains(gridHole.waterHoleID)) newWaterbodies.Add(gridHole.waterHoleID);
                        }
                        holesToCheck.Add(tile);
                        checkedHoles.Add(tile);
                    }
                }
                holesToCheck.RemoveAt(i);
            }
        }
        waterHole.hasEmptyNeighbors = false;

        setValues:
        waterHole.IncreaseNumberOfTiles(selectedHoles.Count);
        int newWaterLevel = waterHole.GetWaterLevel();
        for (int i = 0; i < selectedHoles.Count; i++)
        {
            GridTile tile = selectedHoles[i];
            tile.GridObjectIsType(out GridHole gridHole);
            gridHole.waterLevel = newWaterLevel;
            gridHole.waterHoleID = (int)waterHole.waterBodyID;
       //     GridVisualization.instance.UpdateMesh(tile.x, tile.y, true);
        }
        hole.waterLevel = newWaterLevel;
       // GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, true);

        CompileWaterBody(newWaterbodies, gridTile, waterHole);
    }
    private bool CheckWaterBody(GridTile gridTile,GridTile newHole)
    {
        List<GridTile> holesToCheck = new List<GridTile>();
        List<GridTile> checkedHoles = new List<GridTile>();
        gridTile.GridObjectIsType(out GridHole hole);
        if (hole == null) return false;

        holesToCheck.Add(gridTile);
        checkedHoles.Add(gridTile);

        int tileCount = 1;

        while (holesToCheck.Count > 0)
        {
            for (int i = holesToCheck.Count - 1; i >= 0; i--)
            {
                GridTile holeObj = holesToCheck[i];
                for (int j = 0; j < 4; j++)
                {
                    GridTile tile = GridVisualization.instance.GetGridTileByPositionXY(holeObj.GetXYPosition() + MyTools.directions4[j]);
                    if (tile != null && tile.GridObjectIsType(out GridHole gridHole) && !checkedHoles.Contains(tile) && newHole != tile)
                    {
                        if (gridHole.waterHoleID == hole.waterHoleID)
                        {
                            tileCount++;
                        }
                        holesToCheck.Add(tile);
                        checkedHoles.Add(tile);
                    }
                }
                holesToCheck.RemoveAt(i);
            }
        }

        WaterBody oldWaterBody = waterBodies[hole.waterHoleID];

        if (tileCount != oldWaterBody.tileCount)
        {
            int id = GetNewID();
            int oldId = hole.waterHoleID;

            WaterBody waterHole = new WaterBody(id, tileCount * oldWaterBody.GetFillTile(), tileCount);
            waterBodies.Add(id, waterHole);
            oldWaterBody.Decrease(tileCount * oldWaterBody.GetFillTile(), tileCount);

            int waterlevel = waterHole.GetWaterLevel();

            foreach (GridTile item in checkedHoles)
            {
                Debug.Log(item.ToString());
                if (item != null && item.GridObjectIsType(out GridHole gridHole))
                {
                    if (gridHole.waterHoleID == oldId)
                    {
                        gridHole.waterHoleID = id;
                        if(gridHole.waterLevel != waterlevel)
                        {
                            gridHole.waterLevel = waterlevel;
                            GridVisualization.instance.UpdateMesh(item.x, item.y, true);
                        }
                    }
                }
            }
            return true;
        }
        else
            return false;
    }
}
