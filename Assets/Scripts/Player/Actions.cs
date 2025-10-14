
using System;
using UnityEngine;

public class Actions : MonoBehaviour
{
    [SerializeField] GameObject pointer;
    [SerializeField] Transform parent;
    Transform pointerTransform;
    //public static Grid<GridTile> grid { private set; get; }
    public static Actions instance { private set; get; }
   
    public event EventHandler useItem;

    Vector2 lastPos;

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
    }

    private void Start()
    {
        pointerTransform = Instantiate(pointer, parent).transform;
    }

    public static Vector2 GetMousePosXY()
    {
        return instance.lastPos;
    }
    private void Update()
    {
        if (UIManager.instance.WindowsAreClosed())
        {
           // Vector2 pos = GridVisualization.instance.GetGridPosition(MyTools.GetMouseWorldPosition());
            //if (pos != lastPos)
            //{
            //    lastPos = pos;
            //    SetTileInfo();
            //}

            //if (Input.GetMouseButtonDown(1))
            //{
            //    var tile = GridVisualization.instance.GetTileByGridPosition(pos);
            //    if (tile != null)
            //    {
            //        GridObject gridObject = tile.gridObject;
            //        switch (gridObject)
            //        {
            //            case GridDoor:
            //                Door(gridObject as GridDoor, pos);return;
            //            case GridContainer:
            //                Container(gridObject as GridContainer);return;
            //        }
                    
            //    }
            //}
        }
    }
    private void SetTileInfo()
    {
        pointerTransform.position = GridVisualization.instance.GetWorldPosition(lastPos);
        UIManager.instance.SetCurretTile(GridVisualization.instance.GetGridTileByPositionXY(lastPos));
    }
    private void Door(GridDoor gridDoor,Vector2 position)
    {

        if (gridDoor.doorIsClosed)
            BuildingManager.instance.ChangeSprite(position, 1);
        else
            BuildingManager.instance.ChangeSprite(position, 0);
        gridDoor.doorIsClosed = !gridDoor.doorIsClosed;
    }
    private void Container(GridContainer gridContainer)
    {
        EquipmentManager.instance.LoadChest(gridContainer);
    } 
    public void Action(ItemSlot itemStats)
    {
        Vector2 pos = GridVisualization.instance.GetGridPosition(MyTools.GetMouseWorldPosition());
        GridTile gridTile = GridVisualization.instance.GetTileByGridPosition(pos);
        Item item = null;

        if (itemStats != null) item = ItemsAsset.instance.GetItem(itemStats.itemID);
        else return;
        if (gridTile == null ) return;

        GridObject gridObject;
        if (gridTile.IsGridObjectClass(out gridObject) && !(gridObject is GridHole) && item is Tool)
        {
            var type = ItemsAsset.instance.GetToolRequired(gridObject.ID);
            if (type != ToolType.None && (item == null || type != ((Tool)item)?.toolType))
            {
                Sounds.instance.Sword();
                return;
            }

            Sounds.instance.Shield();
            Transform obj = gridObject.objectTransform;
            float lastRotation = transform.eulerAngles.z;

            VariantItem variantItem = ItemsAsset.instance.GetItem(gridObject.ID) as VariantItem;
            if (variantItem.HitParticles != null) Instantiate(variantItem.HitParticles, gridObject.objectTransform.position + (Vector3)variantItem.objectVariants[gridObject.variantIndex].variants[0].particlePoint, Quaternion.identity);
            GridTile[] neighbors = gridTile.GetNeighbors();

            if (gridTile.DecreaseHitPoints(20))
            {
                Timer.Create(
                () =>
                {
                    float scaleX = Mathf.LerpAngle(obj.localScale.x, 1.1f, Time.deltaTime * 20f);
                    float scaleY = Mathf.LerpAngle(obj.localScale.y, 1.05f, Time.deltaTime * 18f);
                    obj.localScale = new Vector3(scaleX, scaleY);
                    if (obj.localScale.x >= 1.09f)
                    {
                        return true;
                    }


                    return false;
                },
                () =>
                {
                    float scaleX = Mathf.LerpAngle(obj.localScale.x, 1f, Time.deltaTime * 30);
                    float scaleY = Mathf.LerpAngle(obj.localScale.y, 1f, Time.deltaTime * 25);
                    obj.localScale = new Vector3(scaleX, scaleY);
                    if (obj.localScale.x > 0.99f)
                    {
                        obj.localScale = new Vector2(1f, 1f);
                        for (int i = 0; i < 8; i++)
                        {
                            if (neighbors[i] != null)
                            {
                                Transform obj = neighbors[i].gridObject.objectTransform;
                                obj.localScale = new Vector3(1f, 1f);
                            }
                        }
                        return true;
                    }

                    return false;
                }
                );
            }
        }
        else if (item != null)
        {

            switch (item)
            {
                case LiquidContainer:
                    FillLiquidContainer((LiquidContainerItem)itemStats, gridTile);
                    break;
                case Tool:
                    ToolAction(pos,(Tool)item);
                    break;
                case Seed:
                    SeedAction(pos,item);
                    break;
            }

        }
    }

    private void Hit()
    {

    }

    private void ToolAction(Vector2 pos, Tool tool)
    {
        switch (tool.toolType)
        {
            case ToolType.Shovel:
                BuildingManager.instance.Digging(pos);
                break;
            case ToolType.Hoe:
                BuildingManager.instance.Hoeing(pos);
                break;
        }
    }
    public void SideAction(ItemSlot itemStats)
    {

        Vector2 pos = GridVisualization.instance.GetGridPosition(MyTools.GetMouseWorldPosition());
        GridTile gridTile = GridVisualization.instance.GetTileByGridPosition(pos);
        Tool item = null;
        if (itemStats != null) item = ItemsAsset.instance.GetItem(itemStats.itemID) as Tool;
        if (gridTile == null) return;
       
        switch (itemStats)
        {
            case LiquidContainerItem:
                PourWater((LiquidContainerItem)itemStats, gridTile);
                break;
        }      
    }

    private void SeedAction(Vector2 posXY, Item item)
    {
        BuildingManager.instance.Seeding(posXY,(Seed)item);
    }
    private void FillLiquidContainer(LiquidContainerItem item, GridTile gridTile)
    {
        float free = item.GetFreeFill();
        if(free > 0 && gridTile.GridObjectIsType(out GridHole hole) && hole.waterHoleID >= 0)
        {
            float water = LiquidsManager.instance.DecreaseWater(hole.waterHoleID, gridTile, free);
            item.Inecrease(water);
        }
    }
    private void PourWater(LiquidContainerItem item, GridTile gridTile)
    {
        float water = item.currentFill;
        if (water == 0) return;
        switch (gridTile.gridObject)
        {
            case GridHole:
                LiquidsManager.instance.WaterTransfer(gridTile, water);
                item.Decrease(water);
                break;
            case GridFarmland:
                WaterFarmland(item, gridTile);
                break;
        }
    }

    private void WaterFarmland(LiquidContainerItem item, GridTile gridTile)
    {
        gridTile.GridObjectIsType(out GridFarmland farmland);
        farmland.Water();
        Vector2 pos = GridVisualization.instance.GetWorldPosition(gridTile.x, gridTile.y);
        Instantiate(ParticleAssets.instance.water, pos + new Vector2(0,0.14f), Quaternion.identity);
       // GridVisualization.instance.UpdateMesh(gridTile.x, gridTile.y, false,true);
        item.Decrease(50);
    }
}
