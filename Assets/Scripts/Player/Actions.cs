
using UnityEngine;

public class Actions : MonoBehaviour
{
    [SerializeField] GameObject pointer;
    [SerializeField] Transform parent;
    Transform pointerTransform;
    //public static Grid<GridTile> grid { private set; get; }
    public static Actions instance { private set; get; }

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
            Vector2 pos = GridVisualization.instance.GetGridPosition(MyTools.GetMouseWorldPosition());
            if (pos != lastPos)
            {
                lastPos = pos;
                pointerTransform.position = GridVisualization.instance.GetWorldPosition(pos);
            }

            if (Input.GetMouseButtonDown(1))
            {
                var tile = GridVisualization.instance.GetValueByGridPosition(pos);
                if (tile != null)
                {
                    GridObject gridObject = tile.gridObject;
                    switch (gridObject)
                    {
                        case GridDoor:
                            Door(gridObject as GridDoor, pos);return;
                        case GridContainer:
                            Container(gridObject as GridContainer);return;
                    }
                    
                }
            }
        }
    }

    private void Door(GridDoor gridDoor,Vector2 position)
    {
        Debug.Log($"liczba");

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
    public void Destroy(ItemStats itemStats)
    {
        if (itemStats != null)
        {
            Vector2 pos = GridVisualization.instance.GetGridPosition(MyTools.GetMouseWorldPosition());
            GridTile gridTile = GridVisualization.instance.GetValueByGridPosition(pos);
            GridObject gridObject = gridTile?.gridObject;
            if (gridObject != null)
            {
                Sounds.instance.Shield();
                Weapon item = ItemsAsset.instance.GetItem(itemStats.itemID) as Weapon;

                Transform obj = gridObject.objectTransform;
                float lastRotation = transform.eulerAngles.z;
                if (item != null)
                {
                    VariantItem variantItem = ItemsAsset.instance.GetItem(gridObject.ID) as VariantItem;
                    if(variantItem.HitParticles != null) Instantiate(variantItem.HitParticles, gridObject.objectTransform.position + (Vector3)variantItem.objectVariants[gridObject.variantIndex].variants[0].particlePoint, Quaternion.identity);
                    GridTile[] neighbors = gridTile.GetNeighbors();

                    if(gridTile.DecreaseHitPoints(20))
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
            }
        }
    }
}
