using UnityEngine;
using System;
using Unity.Mathematics;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance { private set; get; }

    public event EventHandler builtObject;


    [SerializeField] GameObject pointer;
    [SerializeField] GameObject buildingPrefab;
    [SerializeField] GameObject buildingBar;
    [SerializeField] GameObject shadow;
    private Transform barValue;

    [SerializeField] Transform parent;
    [SerializeField] Color planColor;

    Vector2 startPos;
    int selectedObjectID = -1;
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

    Action<Vector2> build;
    Vector2 lastPos;

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
                    build(pos);
                    planObject.gameObject.SetActive(false);
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
        planObject.gameObject.SetActive(true);
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
        if (obj.IsBuildObject())
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
        planObject.transform.position = GridVisualization.instance.GetWorldPosition(pos);
        var value = GridVisualization.instance.GetValueByGridPosition(pos);
        if(value != null && value.IsBuildObject())
            planObject.gameObject.SetActive(false);
        else
            planObject.gameObject.SetActive(true);
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
        if (gridTile == null || gridTile.IsBuildObject()) return;    
        Sounds.instance.Hammer();
        Transform obj = Instantiate(buildingPrefab,GridVisualization.instance.GetWorldPosition(posXY), Quaternion.identity, parent).transform;
        obj.tag = "BuildObject";
        gridTile.SetGridObject(new Wall(selectedObjectID,0,obj));
        GridVisualization.instance.SetNewSprite(posXY,selectedObjectID);
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
            Transform obj = Instantiate(buildingPrefab, GridVisualization.instance.GetWorldPosition(gridPosition), Quaternion.identity, parent).transform.GetChild(0);
            obj.tag = "BuildObject";
            Transform shadowT = Instantiate(shadow, obj.parent).transform;
            ObjectVariant objectVariant = ItemsAsset.instance.GetObjectVariant(gridObject.ID,gridObject.variantIndex);
            obj.GetComponent<SpriteRenderer>().sprite = objectVariant.variants[0].sprite;
            obj.GetComponent<PolygonCollider2D>().points = objectVariant.variants[0].hitbox;

            CreateGridObject(gridObject.ID,gridPosition, gridObject.variantIndex, obj.parent);
            MyTools.ChangePositionPivot(obj.parent, obj.TransformPoint(0, objectVariant.variants[0].minY, 0));
            shadowT.localPosition = obj.localPosition;
        }
    }
    private void BuildFloor(Vector2 posXY)
    {
        GridTile gridTile = GridVisualization.instance.GetValueByGridPosition(posXY);
        if (gridTile.tileID != selectedObjectID)
        {
            GridVisualization.instance.GetValueByGridPosition(posXY).tileID = selectedObjectID;
            GridVisualization.instance.UpdateMesh((int)posXY.x, (int)posXY.y, true);
            Sounds.instance.Hammer();
            builtObject(this, null);
        }
    }
    private void BuildObject(Vector2 posXY)
    {
        if (GridVisualization.instance.GetValueByGridPosition(posXY).IsBuildObject()) return;

        Sounds.instance.Hammer();
        Transform obj = Instantiate(buildingPrefab, GridVisualization.instance.GetWorldPosition(posXY), Quaternion.identity, parent).transform.GetChild(0);
        obj.tag = "BuildObject";
        Transform shadowT = Instantiate(shadow, obj.parent).transform;

        ObjectVariant objectVariant = ItemsAsset.instance.GetObjectVariant(selectedObjectID, rotation % rotationStates);

        obj.GetComponent<SpriteRenderer>().sprite = objectVariant.variants[0].sprite;
        obj.GetComponent<PolygonCollider2D>().points = objectVariant.variants[0].hitbox;
        CreateGridObject(selectedObjectID,posXY, rotation % rotationStates, obj.parent);
        MyTools.ChangePositionPivot(obj.parent, obj.TransformPoint(0, objectVariant.variants[0].minY, 0));
        shadowT.localPosition = obj.localPosition;
        builtObject(this, null);
    }
    private void CreateGridObject(int itemID,Vector2 posXY,int indexVariant, Transform buildingObj)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        GridTile gridObject = GridVisualization.instance.GetValueByGridPosition(posXY);

        switch (item)
        {
            case DoorItem:
                gridObject.SetGridObject(new GridDoor(itemID, indexVariant, buildingObj), true);
                return;
            case ContainerItem : 
                gridObject.SetGridObject(new GridContainer(itemID, indexVariant, buildingObj,(item as ContainerItem).capacity),true);
                return;
        }

        gridObject.SetGridObject(new GridObject(itemID, indexVariant, buildingObj));
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
