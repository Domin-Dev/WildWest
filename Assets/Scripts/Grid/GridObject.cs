
using Unity.Mathematics;
using UnityEngine;

public interface IGetBarValue
{
    public float GetBarValue();
    public void IncreaseHitPoints(float value);
    public bool DecreaseHitPoints(float value);
}
public class GridContainer : GridObject
{
    public ItemStats[] items;
    public GridContainer(int ID, int indexVariant, Transform obj, int size,Vector2 mainPosition) : base(ID, indexVariant, obj, mainPosition)
    {
        items = new ItemStats[size];
    }
}
public class GridDoor : GridObject
{
    public bool doorIsClosed;
    public GridDoor(int ID, int indexVariant, Transform obj, Vector2 mainPosition, bool doorIsClosed = true) : base(ID, indexVariant, obj,mainPosition)
    {
        this.doorIsClosed = doorIsClosed;
    }
}

public class GridWall : GridObject
{
    public GridWall(int ID, int indexVariant, Transform obj,Vector2 mainPosition, int stateIndex = 0) : base(ID, indexVariant, obj, mainPosition , stateIndex)
    {
    }
}

public class GridSurface : GridObject
{
    public GridSurface(int ID) : base(ID){}
    public override void Destory(GridTile gridTile)
    {
        GridVisualization.instance.DestroySurface(gridTile);
    }
}

public class GridHole : GridObject
{
    public int waterHoleID;
    public int waterLevel;
    public GridHole(int ID,Transform objectTransform, int waterHoleID = -1, int waterLevel = 0) : base(ID, objectTransform) 
    {
        this.waterHoleID = waterHoleID;
        this.waterLevel = waterLevel;
    }

    public void PourWater(float value,GridTile gridTile)
    {
        LiquidsManager.instance.WaterTransfer(gridTile, value);
    }

    public int GetMaxFill()
    {
        return ItemsAsset.instance.GetItem<Hole>(ID).capacity;
    }

}

public class GridObject: IGetBarValue
{
    public int ID;
    public int variantIndex;
    public int stateIndex;
    public Transform objectTransform;

    public float hitPoints;
    private float maxHitPoints;

    public Vector2 mainPosition;

    public GridObject(int ID,int indexVariant, Transform obj, Vector2 mainPosition, int stateIndex = 0)
    {
        this.ID = ID;
        this.maxHitPoints = (ItemsAsset.instance.GetItem(ID) as BuildingItem).durability;
        this.hitPoints = maxHitPoints;
        this.variantIndex = indexVariant;
        this.objectTransform = obj;
        this.stateIndex = stateIndex;
        this.mainPosition = mainPosition;
    }

    public GridObject(int ID)
    {
        this.ID = ID;
        this.maxHitPoints = (ItemsAsset.instance.GetItem(ID) as BuildingItem).durability;
        this.hitPoints = maxHitPoints;

        this.variantIndex = -1;
        this.objectTransform = null;
        this.stateIndex = 0;
        this.mainPosition = Vector2.zero;
    }

    public GridObject(int ID, Transform objectTransform)
    {
        this.ID = ID;
        this.maxHitPoints = (ItemsAsset.instance.GetItem(ID) as BuildingItem).durability;
        this.hitPoints = maxHitPoints;

        this.variantIndex = -1;
        this.objectTransform = objectTransform;
        this.stateIndex = 0;
        this.mainPosition = Vector2.zero;
    }
    public float GetBarValue()
    {
        return hitPoints / maxHitPoints;
    }
    public bool DecreaseHitPoints(float value)
    {
        hitPoints = Mathf.Clamp(hitPoints - value, 0, maxHitPoints);
        if(hitPoints == 0)
        {
            return false;
        }
        return true;
    }
    public void IncreaseHitPoints(float value)
    {
        hitPoints = Mathf.Clamp(hitPoints + value, 0, maxHitPoints);
    }

    public virtual void Destory(GridTile gridTile)
    {
        GridVisualization.instance.DestroyObject(gridTile,true);
    }

    public override string ToString()
    {
        return ItemsAsset.instance.GetItem(ID).name;
    }
}




