using System.Numerics;
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
    public GridContainer(int ID, int indexVariant, Transform obj, int size) : base(ID, indexVariant, obj)
    {
        items = new ItemStats[size];
    }
}
public class GridDoor : GridObject
{
    public bool doorIsClosed;
    public GridDoor(int ID, int indexVariant, Transform obj, bool doorIsClosed = true) : base(ID, indexVariant, obj)
    {
        this.doorIsClosed = doorIsClosed;
    }
}

public class Wall : GridObject
{
    public Wall(int ID, int indexVariant, Transform obj, int stateIndex = 0) : base(ID, indexVariant, obj, stateIndex)
    {
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
    
    public GridObject(int ID,int indexVariant,Transform obj,int stateIndex = 0)
    {
        this.ID = ID;
        this.maxHitPoints = (ItemsAsset.instance.GetItem(ID) as BuildingItem).durability;
        this.hitPoints = maxHitPoints;
        this.variantIndex = indexVariant;
        this.objectTransform = obj;
        this.stateIndex = stateIndex;
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
}




