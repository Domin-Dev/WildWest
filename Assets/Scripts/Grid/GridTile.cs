
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GridTile : IGetBarValue
{
    public int tileID {private set; get; }
    public int secondLayerID { private set; get; }

    public int borders;
    public int variant;
    public GridObject gridObject { private set; get; }
    public int x, y;

    public List<SpriteRenderer> objectsCovering;

    //pathfinding
    public int gCost;
    public int hCost;
    public int fCost;

    public GridTile cameFrom;

    public bool isWalkable;
    public enum TileType
    {

    };
    public GridTile(int x, int y)
    {
        this.tileID = -1;
        this.borders = 0;
        this.x = x;
        this.y = y;
        ResetNode();
        this.isWalkable = true;
    }

    public void SetTileID(int tileID)
    {
        this.tileID=tileID;
    }
    public void SetSecondLayerID(int secondLayerID)
    {
        if (secondLayerID != -1 && ItemsAsset.instance.CheckItemType<FirstLayerFloor>(secondLayerID, out FirstLayerFloor floor))
            this.secondLayerID = floor.swapForSecondLayerFloorID;
        else
            this.secondLayerID = secondLayerID;
    }
    public void SetTileID(int tileID,int secondLayerID)
    {
        this.tileID = tileID;
        this.secondLayerID = secondLayerID;
    }

    public bool IsGridObjectClass(out GridObject gridObjectout)
    {
        gridObjectout = gridObject;
        return IsGridObjectClass();
    }
    public bool IsGridObjectClass()
    {
        if (gridObject == null) return false;
        return gridObject.GetType() != typeof(GridSurface);
    }

    public void SetGridObject(GridObject gridObject, bool isWalkable = false)
    {
        this.gridObject = gridObject;
        this.isWalkable = isWalkable;
    }
    public void SetObjectCovering(SpriteRenderer spriteRenderer)
    {
        if(objectsCovering != null)
        {
            objectsCovering.Add(spriteRenderer);
        }
        else
        {
            objectsCovering = new List<SpriteRenderer>();
            objectsCovering.Add(spriteRenderer);
        }

    }

    public void TrunOffObjectsCovering()
    {
        if(objectsCovering != null)
        {
            for (int i = objectsCovering.Count - 1; i >= 0; i--)
            {
                SpriteRenderer spriteRenderer = objectsCovering[i];
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 0.5f;
                    spriteRenderer.color = color;
                }
                else
                {
                    objectsCovering.RemoveAt(i);
                    if (objectsCovering.Count == 0) objectsCovering = null;
                }
            }
        }

    }
    public void TurnOnObjectsCovering()
    {
        if (objectsCovering != null)
        {
            for (int i = objectsCovering.Count - 1; i >= 0; i--)
            {
                SpriteRenderer spriteRenderer = objectsCovering[i];
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
                else
                {
                    objectsCovering.RemoveAt(i);
                    if (objectsCovering.Count == 0) objectsCovering = null;
                }
            }
        }
    }
    public void CalculateFCost()
    {
        fCost = hCost + gCost;
    }

    public void ResetNode()
    {
        gCost = int.MaxValue;
        cameFrom = null;
        CalculateFCost();
    }
    public Vector2 GetXYPosition()
    {
        return new Vector2(x, y);
    }
    public void ChangeTileType(int tileID)
    {
        this.tileID = tileID;
        GridVisualization.instance.TileChanged(x, y);
    }
    public void ChangeGridObject(GridObject gridObject)
    {
        this.gridObject = gridObject;
        GridVisualization.instance.TileChanged(x, y);
    }
    public bool IsBuildObject()
    {
        return gridObject != null;
    }
    public bool IsBuildObject(int id)
    {  
        return IsBuildObject() && gridObject.ID == id; 
    }
    
    public bool GridObjectIsType<T>()
    {
        if(gridObject == null) return false;
        else
        {
            return gridObject is T;
        }
    }
    public bool GridObjectIsType<T>(out T output) where T : GridObject
    {
        output = null;
        if (gridObject == null) return false;
        else
        {
            output = gridObject as T;
            return gridObject is T;
        }
    }
    public override string ToString()
    {
        return $"Position : [{x},{y}]";
    }

    public string GetTileInfo()
    {
        string output = "";

        if (IsGridObjectClass())
        {
            if (GridObjectIsType<GridHole>(out GridHole hole))
            {
                double fill = LiquidsManager.instance.GetFill(hole.waterHoleID);
                if( fill  > 0)
                    output += "<color=#A3A3A3>Water: " + "[" + fill.ToString("F2") + "/"+ ItemsAsset.instance.GetItem<Hole>(tileID).capacity +"]" + "</color> " + hole.waterHoleID  +" \n";
                else
                    output += "Hole";
                return output;
            }
            output += gridObject.ToString() + '\n';
        }
        if(tileID >= 0) output += "Tile: " + ItemsAsset.instance.GetItem(tileID).name + '\n';
        if(secondLayerID >= 0) output += "<color=#A3A3A3>Second Layer: " + ItemsAsset.instance.GetItem(secondLayerID).name + "</color> \n";

        return output;
    }


    public float GetBarValue()
    {
       return gridObject.GetBarValue();
    }
    public void IncreaseHitPoints(float value)
    {
       gridObject.IncreaseHitPoints(value);       
    }
    public bool DecreaseHitPoints(float value)
    {
        if(!gridObject.DecreaseHitPoints(value))
        {
            gridObject.Destory(this);
            return false;
        }
        return true;
    }
    public GridTile[] GetNeighbors()
    {
        GridTile[] neighbors = new GridTile[8];
        for (int i = 0; i < 8; i++)
        {
           var obj = GridVisualization.instance.GetValueByGridPosition(new Vector2(x,y) + MyTools.directions8[i]);
           if(obj != null && obj.gridObject != null && obj.gridObject.objectTransform != null) neighbors[i] = obj;
        }
        return neighbors;
    }

    
}