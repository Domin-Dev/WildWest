
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GridContainer : GridObject
{
    public ItemSlot[] items;
    public GridContainer(int ID, int indexVariant, Transform obj, int size,Vector2 mainPosition) : base(ID, indexVariant, obj, mainPosition)
    {
        items = new ItemSlot[size];
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
    public int GetMaxFill()
    {
        return ItemsAsset.instance.GetItem<Hole>(ID).capacity;
    }

}
public class GridObject: IHitPoints
{
    public int ID;
    public short variantIndex;
    public short stateIndex;
    public Transform objectTransform;

    public float hitPoints;
    private float maxHitPoints;

    public Vector2 mainPosition;

    public GridObject(int ID,int indexVariant, Transform obj, Vector2 mainPosition, int stateIndex = 0)
    {
        SetObject(ID,(short)indexVariant, obj, mainPosition,(short)stateIndex);
    }

    public GridObject(byte[] bytes)
    {
        this.ID =  BitConverter.ToInt32(bytes, 0);
        this.variantIndex =  BitConverter.ToInt16(bytes, 4);
        this.stateIndex =  BitConverter.ToInt16(bytes, 6);

        this.hitPoints =  BitConverter.ToSingle(bytes, 8);
        this.maxHitPoints =  BitConverter.ToSingle(bytes, 12);
        this.mainPosition.x = BitConverter.ToSingle(bytes, 16);
        this.mainPosition.y = BitConverter.ToSingle(bytes, 20);
    }
    protected void SetObject(int ID, short indexVariant, Transform obj, Vector2 mainPosition, short stateIndex)
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

    public virtual byte[] GetBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(ID));
        bytes.AddRange(BitConverter.GetBytes(variantIndex));
        bytes.AddRange(BitConverter.GetBytes(stateIndex));
        bytes.AddRange(BitConverter.GetBytes(hitPoints));
        bytes.AddRange(BitConverter.GetBytes(maxHitPoints));
        bytes.AddRange(BitConverter.GetBytes(mainPosition.x));
        bytes.AddRange(BitConverter.GetBytes(mainPosition.y));
        return bytes.ToArray();
    }



    public override string ToString()
    {
        return ItemsAsset.instance.GetItem(ID).name + " " + this.variantIndex + " " + this.mainPosition;
    }

}
public class GridFarmland : GridSurface, IWater
{
    private bool watered;
    public GridFarmland(int ID,bool watered = false) : base(ID)
    {
        this.watered = watered;
    }
    public void Water()
    {
        watered = true;
    }
    public bool IsWatered()
    {
        return watered;
    }
    public void Dry()
    {
        watered = false;
    }
}
public class GridPlant : GridObject,IWater
{
    private bool watered;
    private float toGrowth;
    public GridPlant(bool watered,int ID, int indexVariant, Transform obj, Vector2 mainPosition, int stateIndex = 0): base(ID,indexVariant,obj,mainPosition,stateIndex)
    {
        this.watered = watered;
        toGrowth = 0;
        TimeTickSystem.OnTick += Tick;   
    }

    private void Tick(object sender, TimeTickSystem.OnTickArgs e)
    {
        int stage = GetCurrentStage(toGrowth);
        if(watered)
            toGrowth += 0.01f;
        else
            toGrowth += 0.005f;

        int newStage = GetCurrentStage(toGrowth);
        if(stage != newStage)
        {
            if(newStage == 4) 
            {
                TimeTickSystem.OnTick -= Tick;        
            }
            variantIndex = (short)newStage;
            BuildingManager.instance.ChangeSprite(this);
        }
    }

    private int GetCurrentStage(float toGrowth)
    {
        if (toGrowth >= 1) return 4;
        else if (toGrowth >= 0.7f) return 3;
        else if (toGrowth >= 0.5f) return 2;
        else if (toGrowth >= 0.1f) return 1;
        else  return 0;
    }
    public void Water()
    {
        watered = true;
    }
    public bool IsWatered()
    {
        return watered;
    }
    public void Dry()
    {
        watered = false;
    }
    public override void Destory(GridTile gridTile)
    {
        GridVisualization.instance.DestroyPlant(gridTile);
    }
}


