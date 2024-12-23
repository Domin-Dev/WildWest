using UnityEngine;
using System;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance { private set; get; }

    public event EventHandler builtObject;


    [SerializeField] GameObject pointer;
    [SerializeField] GameObject buildingPrefab;
    [SerializeField] GameObject buildingBar;
    [SerializeField] GameObject collider;

    private Transform barValue;

    [SerializeField] Transform parent;
    [SerializeField] Color planColor;

    Vector2 startPos;
    Vector2 shadowOffset;
    Action<Vector2> build;
    Vector2 lastPos;


    int _selectedObjectID;
    int selectedObjectID
    {
        set
        {
            if (value >= 0)
            {
                shadowOffset = ItemsAsset.instance.GetOffsetVector(value);
                if (lastPos != null) Plan(lastPos);
            }
            _selectedObjectID = value;
        }
        get { return _selectedObjectID; }
    } 

    int rotationStates = 0;
    bool buildingMode;
    int rotation = 0;

    Transform objectBar;
    bool barIsActive;

    Transform planObject;
    Transform pointerObject;

    private void Awake()
    {
   
        if (instance == null)
        {
            instance = this;
            selectedObjectID = -1;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        SetUpBar();
        SetUpPlanObject();
        SetUpPointer();
    }


    private void Update()
    {
        if (buildingMode)
        {
            if (selectedObjectID > 0 && UIManager.instance.WindowsAreClosed())
            {
                Vector2 pos = Actions.GetMousePosXY();
                if (lastPos != pos) Plan(pos);
                if (Input.GetMouseButtonDown(0))
                {
                    if (GridVisualization.instance.GetValueByGridPosition(pos) != null)
                    {
                        build(pos);
                        planObject.gameObject.SetActive(false);
                        UIManager.instance.PrintTileInfo();
                    }
                }
                if (Input.GetKeyDown(KeyCode.R) && rotationStates > 0)
                {
                    if(rotation >= 3)
                    {
                        rotation = 0;
                    }
                    else
                    {
                        rotation++;
                    }
                    planObject.GetComponent<SpriteRenderer>().sprite = ItemsAsset.instance.GetBuildingObjectSprite(selectedObjectID,rotation%rotationStates);

                }
            }
        }
    }

    public void StartBuildingMode(int id)
    {
        selectedObjectID = id;
 
        SetBuildMode();
        SpriteRenderer spriteRenderer = planObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = ItemsAsset.instance.GetBuildingObjectSprite(id,rotation % rotationStates); 
        
        buildingMode = true;
    }
    public void EndBuildingMode()
    {
        buildingMode = false;
        planObject.gameObject.SetActive(false);
        selectedObjectID = -1;
    }
    private void SetUpBar()
    {
        objectBar = Instantiate(buildingBar, Vector3.one, Quaternion.identity).transform;
        objectBar.gameObject.SetActive(false);
        barValue = objectBar.GetChild(0).GetChild(0);
        barIsActive = false;
    }
    private void SetUpPlanObject()
    {
        planObject = Instantiate(buildingPrefab, Vector2.zero, Quaternion.identity, parent).transform.GetChild(0);
        planObject.GetComponent<Collider2D>().enabled = false;
        planObject.GetComponent<SpriteRenderer>().color = planColor;
        planObject.gameObject.SetActive(false);
    }
    private void SetUpPointer()
    {
        pointerObject = Instantiate(pointer, parent).transform;
        pointerObject.gameObject.SetActive(false);
    }
    private void UpdateBar()
    {
        var obj = GridVisualization.instance.GetValueByGridPosition(startPos);
        if (obj.IsGridObjectClass())
        {
            TurnOnBuildingBar(obj.gridObject.objectTransform.position + new Vector3(0, 0.5f), (obj.gridObject as IGetBarValue).GetBarValue());
        }
        else
        {
            TurnOffBuildingBar();
        }
    }
    private void TurnOnBuildingBar(Vector2 position,float value)
    {
        if (!objectBar.gameObject.activeSelf)
        {
            objectBar.gameObject.SetActive(true);
        }
        objectBar.transform.position = position;
        barValue.localScale = new Vector3(value, 1, 1);
    }
    private void TurnOffBuildingBar()
    {
        if (objectBar.gameObject.activeSelf)
        {
            objectBar.gameObject.SetActive(false);
        }
    }
    private void Plan(Vector2 pos)
    {
        lastPos = pos;
        planObject.transform.position = GridVisualization.instance.GetWorldPosition(pos) - shadowOffset;
        var value = GridVisualization.instance.GetValueByGridPosition(pos);
        
        if (value != null && (value.IsGridObjectClass() || CheckObjectPoints(pos, selectedObjectID)))
            planObject.gameObject.SetActive(false);
        else
        {  
            planObject.gameObject.SetActive(true);
        }
    }

    private bool CheckObjectPoints(Vector2 pos,int id)
    {
        Item item = ItemsAsset.instance.GetItem(id);
        if(!(item is VariantItem)) return false;
        VariantItem variantItem = item as VariantItem;
        if (variantItem == null) return false;
        Variant variant = variantItem.objectVariants[rotation % rotationStates].variants[0];

        for (int i = 0; i < variant.objectPoints.Length; i++)
        {
            var tile = GridVisualization.instance.GetValueByGridPosition(pos + variant.objectPoints[i]);
            if(tile == null || tile.IsGridObjectClass()) return true;
        }
        return false;
    }
    private void SetBuildMode()
    {
        Item item = ItemsAsset.instance.GetItem(selectedObjectID);
        if (item is WallObject) build = BuildWall;
        else if (item is Floor) build = BuildFloor;
        else if (item is BuildingObject) build = BuildObject;

        if (item is BuildingObject) rotationStates = (item as BuildingObject).objectVariants.Length;
        else
        {
            rotationStates = 1;
            rotation = 0;
        }
    }
    private void BuildWall(Vector2 posXY)
    {
        var gridTile = GridVisualization.instance.GetValueByGridPosition(posXY);
        if (gridTile == null || gridTile.IsGridObjectClass()) return;
        VariantItem item = (VariantItem)ItemsAsset.instance.GetItem(selectedObjectID);
        Variant variant = item.objectVariants[rotation % rotationStates].variants[0];

        Sounds.instance.Hammer();
        Transform obj = Instantiate(buildingPrefab, GridVisualization.instance.GetWorldPosition(posXY) - new Vector2(0f, item.shadowPixels * 0.01f), Quaternion.identity, parent).transform;
        obj.tag = "BuildObject";
        gridTile.SetGridObject(new GridWall(selectedObjectID,0,obj,posXY));
        GridVisualization.instance.SetNewSprite(posXY,selectedObjectID);
        GridVisualization.instance.MoveWorldItems(posXY);
        builtObject(this, null);
    }
    public void LoadObject(GridObject gridObject,Vector2 gridPosition)
    {
        Item item = ItemsAsset.instance.GetItem(gridObject.ID);
        if (item is WallObject)
        { 
            Transform obj = Instantiate(buildingPrefab, GridVisualization.instance.GetWorldPosition(gridPosition), Quaternion.identity, parent).transform;
            gridObject.objectTransform = obj;

        }
        else if(item is BuildingObject)
        {
            VariantItem variantItem = (VariantItem)ItemsAsset.instance.GetItem(gridObject.ID);

            Transform obj = Instantiate(buildingPrefab, GridVisualization.instance.GetWorldPosition(gridPosition) - new Vector2(0f, variantItem.shadowPixels * 0.01f), Quaternion.identity, parent).transform.GetChild(0);
            obj.tag = "BuildObject";

            Variant variant = variantItem.objectVariants[gridObject.variantIndex].variants[0];
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (variant.CoveringPoints != null)
            {
                for (int i = 0; i < variant.CoveringPoints.Length; i++)
                {
                    GridVisualization.instance.GetValueByGridPosition(gridPosition + variant.CoveringPoints[i])?.SetObjectCovering(spriteRenderer);
                }
            }
            spriteRenderer.sprite = variant.sprite;
            obj.GetComponent<PolygonCollider2D>().points = variant.hitbox;
            CreateGridObject(gridObject.ID,gridPosition, gridObject.variantIndex, obj.parent);
            MyTools.ChangePositionPivot(obj.parent, obj.TransformPoint(0, variant.minY, 0));
        }
    }

    public void Digging(Vector2 posXY)
    {
        GridTile gridTile = GridVisualization.instance.GetValueByGridPosition(posXY);
        if (gridTile == null) return;

        if (gridTile.secondLayerID >= 0)
        {
            GridVisualization.instance.CreateWorldItem(new ItemStats(gridTile.tileID), posXY);
            gridTile.SetTileID(gridTile.secondLayerID);
            gridTile.variant = CalculateVariant(gridTile.secondLayerID);
            gridTile.SetGridObject(new GridSurface(gridTile.secondLayerID),true);
            gridTile.SetSecondLayerID(-1);

            GridVisualization.instance.UpdateMesh((int)posXY.x, (int)posXY.y, true);
            Sounds.instance.Hammer();
        }
        else if(!gridTile.GridObjectIsType<GridHole>())
        {
            Floor floor = ItemsAsset.instance.GetItem<Floor>(gridTile.tileID);
            if (floor.diggingParticles != null) Instantiate(floor.diggingParticles, GridVisualization.instance.GetWorldPosition(posXY + new Vector2(0,0.5f)), Quaternion.identity);
            Sounds.instance.Hammer();
            if (!gridTile.DecreaseHitPoints(20))
            {
                GridVisualization.instance.CreateWorldItem(new ItemStats(gridTile.tileID), posXY);
                int id = 60;
                gridTile.SetTileID(60);
                gridTile.variant = CalculateVariant(id);
                Transform obj = Instantiate(collider, GridVisualization.instance.GetWorldPosition(posXY), Quaternion.identity, parent).transform;
                obj.tag = "BuildObject";
                gridTile.SetGridObject(new GridHole(id,obj));
                GridVisualization.instance.UpdateMesh((int)posXY.x, (int)posXY.y, true);
                GridVisualization.instance.NewHole(gridTile);
                GridVisualization.instance.MoveWorldItems(posXY);
            }
        }
    }
    private void BuildFloor(Vector2 posXY)
    {
        GridTile gridTile = GridVisualization.instance.GetValueByGridPosition(posXY);
        if (gridTile.tileID != selectedObjectID || (gridTile.secondLayerID == -1))
        {
            if (gridTile.GridObjectIsType<GridHole>())
            {
                GridVisualization.instance.DestroyObject(gridTile,false);
                gridTile.SetGridObject(new GridSurface(selectedObjectID), true);
            }
            else if(gridTile.tileID != -1)
            {
                if (gridTile.secondLayerID != -1)
                    GridVisualization.instance.CreateWorldItem(new ItemStats(gridTile.tileID), posXY);
                else
                    gridTile.SetSecondLayerID(gridTile.tileID); 
            }

            gridTile.SetTileID(selectedObjectID);
            gridTile.variant = CalculateVariant(selectedObjectID);
            GridVisualization.instance.UpdateMesh((int)posXY.x, (int)posXY.y, true);
            Sounds.instance.Hammer();
            builtObject(this, null);
        }
    }

    private int CalculateVariant(int floorID)
    {
        var tileUV =  GridVisualization.instance.TilesUV[floorID];
        Floor floor =  ItemsAsset.instance.GetItem<Floor>(floorID);
        if (floor != null)
        {
            System.Random random = new System.Random();
            if (tileUV.variants == 1 || random.Next(100) / 99f < floor.chanceOfDefaultTile)
                return 0;
            else
                return random.Next(1,tileUV.variants);
        }
        else
            return 0;
    }
    private void BuildObject(Vector2 posXY)
    {
        if (GridVisualization.instance.GetValueByGridPosition(posXY).IsGridObjectClass()) return;

        VariantItem item = (VariantItem)ItemsAsset.instance.GetItem(selectedObjectID);
        Variant variant = item.objectVariants[rotation % rotationStates].variants[0];


        for (int i = 0; i < variant.objectPoints.Length; i++)
        {
            if (GridVisualization.instance.GetValueByGridPosition(posXY + variant.objectPoints[i]).IsGridObjectClass()) return;
        }

        Sounds.instance.Hammer();
        Transform obj = Instantiate(buildingPrefab, GridVisualization.instance.GetWorldPosition(posXY) - new Vector2(0f,item.shadowPixels * 0.01f), Quaternion.identity, parent).transform.GetChild(0);
        obj.tag = "BuildObject";
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();



        if (variant.CoveringPoints != null)
        {
            for (int i = 0; i < variant.CoveringPoints.Length; i++)
            {
                GridVisualization.instance.GetValueByGridPosition(posXY + variant.CoveringPoints[i]).SetObjectCovering(spriteRenderer);
            }
        }

        spriteRenderer.sprite = variant.sprite;
        obj.GetComponent<PolygonCollider2D>().points = variant.hitbox;
        CreateGridObject(selectedObjectID,posXY, rotation % rotationStates, obj.parent);
        MyTools.ChangePositionPivot(obj.parent, obj.TransformPoint(0, variant.minY, 0));
        GridVisualization.instance.MoveWorldItems(posXY);
        builtObject(this, null);
    }

    private void CreateGridObject(int itemID,Vector2 posXY,int indexVariant, Transform buildingObj)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        GridTile gridObject = GridVisualization.instance.GetValueByGridPosition(posXY);

        switch (item)
        {
            case DoorItem:
                gridObject.SetGridObject(new GridDoor(itemID, indexVariant, buildingObj,posXY), true);
                return;
            case ContainerItem : 
                gridObject.SetGridObject(new GridContainer(itemID, indexVariant, buildingObj,(item as ContainerItem).capacity,posXY),true);
                return;
        }

        GridObject gridObj = new GridObject(itemID, indexVariant, buildingObj,posXY);
        gridObject.SetGridObject(gridObj);

        Variant variant = ((VariantItem)item).objectVariants[indexVariant].variants[0];
        if (variant.objectPoints != null)
        {
            for (int i = 0; i < variant.objectPoints.Length; i++)
            {
                GridVisualization.instance.GetValueByGridPosition(posXY + variant.objectPoints[i])?.SetGridObject(gridObj);
            }
        }
    }
    public void ChangeSprite(Vector2 posXY, int index)
    {
        GridObject gridObject = GridVisualization.instance.GetValueByGridPosition(posXY).gridObject;
        Variant  variant = ItemsAsset.instance.GetObjectVariant(gridObject.ID, gridObject.variantIndex).variants[index];

        Transform obj = null;
        for (int i = 0; i < gridObject.objectTransform.childCount; i++)
        {
            if (gridObject.objectTransform.GetChild(i).CompareTag("BuildObject"))
            obj = gridObject.objectTransform.GetChild(i);
        }

        obj.GetComponent<SpriteRenderer>().sprite = variant.sprite;
        PolygonCollider2D polygonCollider2D = obj.GetComponent<PolygonCollider2D>();
        polygonCollider2D.points = variant.hitbox;
        polygonCollider2D.usedByComposite = false;
        Timer.Create(2f, () => { if(polygonCollider2D != null) polygonCollider2D.usedByComposite = true; return false; });
        MyTools.ChangePositionPivot(gridObject.objectTransform, obj.TransformPoint(0, variant.minY, 0));
    }
}
