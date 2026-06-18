 using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class EquipmentGrid
{
    public Transform gridTransform { private set; get; }
    public int gridIndex {private set; get; }

    public EquipmentGrid(Transform gridTransform, int gridIndex)
    {
        this.gridTransform = gridTransform;
        this.gridIndex = gridIndex;
    }
}

[System.Serializable]
public class ContainerUI
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private ContainerType _containerType;

    public GameObject prefab => _prefab;
    public Transform parent => _parent;
    public ContainerType containerType => _containerType;
}

public class UIManager : MonoBehaviour
{
    //black background
    [SerializeField] private TabUI tabUI;
    [SerializeField] private PlayerStatsUI statsUI;
    [Space(20f)]
    [SerializeField] private Transform background;
    [Space(20f)]
    //equipment
    [SerializeField] private Canvas mainCanvas;
    [Header("Gun Info UI")]
    [SerializeField] private Transform ammoBar;
    [SerializeField] private GameObject ammoUI;



    #region AmmoUI

    [SerializeField] private GameObject ammoCounerPrefab;
    [SerializeField] private Transform ammoCountersParent;
    [SerializeField] private GameObject ammoMagazinePrefab;
    [SerializeField] private Transform ammoMagazineParent;


    #endregion

    #region Equipment UI
    [Header("Equipment UI")]
    [SerializeField] private Transform mainItemBar;
    [SerializeField] private Transform equipment;
    [SerializeField] private TextMeshProUGUI itemInHandPopup;
    [Space]
    [SerializeField] private GameObject itemSlot;
    [SerializeField] private GameObject greyIcon;
    [SerializeField] private GameObject slotIndex;
    [SerializeField] private GameObject item;
    [SerializeField] private GameObject itembar;
    [Header("Container Objects")]
    [SerializeField] private List<ContainerUI> containers; 
    [Header("Container Options")]
    [SerializeField] private GameObject containerInfo;
    [SerializeField] private GameObject containerStack;
    [Space]
    
    [SerializeField] private Transform equipmentDragItems;
    [SerializeField] private Transform equipmentClothes;
    [Space(20f)]
    #endregion
    #region Stats UI
    [Header("Stats UI")]
    [SerializeField] private Bullet ServerButton;
    [SerializeField] private Bullet HostButton;
    [SerializeField] private Bullet ClientButton;
    [SerializeField] Sprite selected;
    [SerializeField] Sprite unSelected;
    #endregion
    #region Crafting UI
    [Header("Crafting UI")]
    [SerializeField] Transform recipeDescription;
    [SerializeField] Transform recipes;
    [SerializeField] GameObject recipeIcon;
    [SerializeField] Transform ingredients;
    #endregion
    [SerializeField] Transform collectedItems;
    [Space(20f)]
    [SerializeField] TextMeshProUGUI tileInfo;
    [Space(20f)]


    private const float buttonScale = 1f;
    private const float selectedButtonScale = 1.1f;

    private const float speedSelecting = 10f;
    private const float speedUnselecting = 15f;

    public Transform itemParent { get { return equipmentDragItems; } }
    public static UIManager instance { private set; get; }

    private EquipmentGrid barGrid;
    private EquipmentGrid equipmentBarGrid;
    private EquipmentGrid mainEquipmentGrid;
    private EquipmentGrid clothesGrid;
    private EquipmentGrid containerGrid;

    private List<Transform> openWindows = new List<Transform>();
            
    public bool WindowsAreClosed => openWindows.Count == 0 && !tabUI.IsOpen() && !ChatManager.instance.isChatting;


    private List<int> loadedScene = new List<int>();
    private List<(Color color, Type type)> colors;
    private Dictionary<string,Property> properties;

    public event EventHandler windowOpen;

    [SerializeField] private UIConfig uISettings;




   
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        SetGrids();
        SetUpNotices();
        LoadRecipes();
        colors = uISettings.GetColors();
        properties = uISettings.GetProperties();
        itemInHandPopup.gameObject.SetActive(false);
        

        List<string> stats = new List<string>();
        var values = properties.Values.OrderByDescending(x => x.DisplayPriority).ToList();

        foreach(var key in values)
        {
            if(key.DisplayEQWindow)
                stats.Add(key.name);       
        }
        statsUI.Load(stats.ToArray());
    }
    public void TabSetUp()
    {
        Debug.Log("setup");
        tabUI.equipmentTab.OnEnable += () =>
        {
            // windowOpen(this,null);
            //openWindows.Add(equipment);
            SelectItem(-1);
            CheckRecipes();
            Debug.Log("dziala!!");
        };  
        
        tabUI.equipmentTab.OnDisable += () =>
        {
            NewEquipmentManager.instance.DeselectItem();
            TooltipSystem.Hide();
            //openWindows.Remove(equipment);
            ResetSelectedItem();        
            if(timer != null) timer.Cancel();
        };

    }

    private void Update()
    {
        UpdateButtonSize();
    }
    public void FixedUpdate()
    {
        MultiCraft();
    }
    public void OnEnable()
    {
        NewEquipmentManager.onNewSlotInHand += UpdateSlotInHand;
        NewItemInHandSystem.onNewItemInHand += NewItemInHand;
    }
    public void OnDisable()
    {
        NewEquipmentManager.onNewSlotInHand -= UpdateSlotInHand;
        NewItemInHandSystem.onNewItemInHand -= NewItemInHand;
    }

    private int i = 0;
    private int k = 0;
    private bool isHold;
    private void MultiCraft()
    {
        if (isHold)
        {
            i++;
            k++;
            if (i >= (7 - k / 20) + 1)
            {
                Craft(selectedID);
                i = 0;
            }
        }
    }

    private void SetGrids()
    {
        barGrid = new EquipmentGrid(mainItemBar, 0);
        clothesGrid = new EquipmentGrid(equipmentClothes, 2);
    }

    public void SetUpUIEquipment(EquipmentManager eqManager)
    { 
        // eqManager.UpdateSelectedSlotInBar += UpdateSelectedSlot;


        // eqManager.CreateItemUI += CreateItemUI;
        // eqManager.MoveItemUI += MoveItemUI;
        // eqManager.RemoveItemUI += RemoveItemUI;
        // eqManager.UpdateItemCount += UpdateItemCount;
        // eqManager.UpdateDragItemCount += UpdateDragItemCount;
        // eqManager.RemoveDragItemUI += RemoveDragItemUI;
        // eqManager.MoveMainBarItem += MoveMainBarItem;
        // eqManager.CreateMainBarItem += CreateMainBarItem;
        // eqManager.RemoveMainBarItem += RemoveMainBarItem;
        // eqManager.UpdateMainBarItemCount += UpdateMainBarItemCount;
        // eqManager.UpdateItemBar += UpdateItemLifeBar;
        // eqManager.TurnPlaceholder += TurnPlaceholder;

      //  LoadClothesSlots(clothesGrid);
    }
    public void LoadSlotsContainer(ItemStats[] items)
    {
        containerGrid.gridTransform.gameObject.SetActive(true);
        if (containerGrid.gridTransform.childCount >= items.Length)
        {
            for (int i = 0; i < containerGrid.gridTransform.childCount; i++)
            {
                if (i >= items.Length) containerGrid.gridTransform.GetChild(i).gameObject.SetActive(false);
                else
                {
                    foreach(Transform child in containerGrid.gridTransform.GetChild(i))
                    { 
                        Destroy(child.gameObject);
                    }
                    containerGrid.gridTransform.GetChild(i).gameObject.SetActive(true);
                    if (items[i] != null) NewItemUI(containerGrid.gridTransform, new CreateItemArgs(items[i], new SlotPosition(3, i), false));
                }
            }
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (i >= containerGrid.gridTransform.childCount)
                {
                    Transform slot = Instantiate(itemSlot, containerGrid.gridTransform).transform;
                    slot.AddComponent<DropSlot>().SetSlotPosition(i, containerGrid.gridIndex);
                }
                else
                {
                    if (containerGrid.gridTransform.GetChild(i).childCount > 0) Destroy(containerGrid.gridTransform.GetChild(i).GetChild(0).gameObject);
                    containerGrid.gridTransform.GetChild(i).gameObject.SetActive(true);
                }
                if (items[i] != null) NewItemUI(containerGrid.gridTransform, new CreateItemArgs(items[i], new SlotPosition(3, i), false));
                
            }
        }
        OpenEquipment(true); 
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerGrid.gridTransform.GetComponent<RectTransform>());
    }


    private GridTile tile;
    public void SetCurretTile(GridTile gridTile)
    {
        tile = gridTile;
        PrintTileInfo();
    }

    public void PrintTileInfo()
    {
        tileInfo.text = "";
        if (tile == null) return;
        else tileInfo.text = tile.GetTileInfo();
    }

    public void LoadSlotsDedicatedContainer(ItemStats[] items,Sprite icon)
    {
        LoadSlotsContainer(items);
        for (int i = 0; i < items.Length; i++)
        { 
            Transform o = containerGrid.gridTransform.GetChild(i);
            Transform grey = Instantiate(greyIcon, o).transform;
            grey.localPosition = Vector3.zero;
            grey.GetComponent<Image>().sprite = icon;
            if (items[i] != null) grey.gameObject.SetActive(false);
        }
    }
    private void TurnPlaceholder(object sender, PlaceholderArgs e)
    {
        SwitchPlaceholder(e.turn, GetGrid(e.slotPosition.containerIndex).GetChild(e.slotPosition.slotIndex));
    }

    private void HideAmmoBar(object sender, EventArgs e)
    {
        ammoBar.gameObject.SetActive(false);
    }
    private void UpdateAmmoBar(object sender, UpdateAmmoBarArgs e)
    {
        if(lastAmmoCount > e.currentCount)
        { 
            for(int i = lastAmmoCount -1;i >= e.currentCount;i--)
            {
                Image ammo = ammoBar.GetChild(i).GetComponent<Image>();
                ammo.color = Color.black;
            }
        }
        else if (lastAmmoCount < e.currentCount)
        {
            int i;
            if (lastAmmoCount > 0) i = lastAmmoCount - 1;
            else i = 0;
            
            for (;i < e.currentCount; i++)
            {
                Image ammo = ammoBar.GetChild(i).GetComponent<Image>();
                ammo.color = Color.white;
            }
        }
        lastAmmoCount = e.currentCount;
    }

    private int lastAmmoCount = -1;
    private void SetAmmoBar(object sender, SetAmmoBarArgs e)
    {
        ammoBar.gameObject.SetActive(true);
        lastAmmoCount = e.currentCount;
        Sprite sprite = ItemsAsset.instance.GetAmmoSpriteUI(e.type);
        for (int i = 0; i < ammoBar.childCount; i++)
        {
            Image ammo= ammoBar.GetChild(i).GetComponent<Image>();
           
            if(i < e.magazineCapacity)
            {
                ammo.gameObject.SetActive(true);
                ammo.sprite = sprite;
                if(i < e.currentCount) ammo.color = Color.white;
                else ammo.color = Color.black;           
            }
            else
                ammo.gameObject.SetActive(false);
            
        }
        int toAdd = e.magazineCapacity - ammoBar.childCount;
        if (toAdd > 0)
        {
            for (int i = 0; i < toAdd; i++)
            {
                Image ammo = Instantiate(ammoUI, ammoBar).GetComponent<Image>();
                ammo.sprite = sprite;
                if (ammoBar.childCount <= e.currentCount) ammo.color = Color.white;
                else ammo.color = Color.black;
            }
        }
    }
    private void UpdateItemLifeBar(object sender, LifeBarArgs e)
    {
        Transform slot = GetItem(e.position);
        FindBar(e.barValue,slot);
        if(e.position.containerIndex == 0)
        {
            slot = GetItemFromMainBar(e.position);
            FindBar(e.barValue, slot);
        }
    }
    private void FindBar(float value,Transform slot)
    {
        if (slot == null) return;
        for (int i = 0; i < slot.childCount; i++)
        {
            if (slot.GetChild(i).CompareTag("Bar"))
            {
                UpdateBar(value, slot.GetChild(i).transform.GetChild(0));
                return;
            }
        }
    }
    private void UpdateBar(float value, Transform barTransform)
    {
        Image bar = barTransform.GetComponent<Image>();
        bar.transform.localScale = new Vector3(value, 1,1);
      //  bar.color = new Color(bar.color.r, value, bar.color.b);
    }



    private void UpdateMainBarItemCount(object sender, UpdateItemCountArgs e)
    {
        UpdateCount(mainItemBar, e);
    }
    private void RemoveMainBarItem(object sender, PositionArgs e)
    {
        RemoveItem(mainItemBar, e);
    }
    private void CreateMainBarItem(object sender, CreateItemArgs e)
    {
        NewItemUI(mainItemBar, e);
    }
    private void MoveMainBarItem(object sender, MoveItemArgs e)
    {
        if(e.from.containerIndex == 0 && e.to.containerIndex == 0)
        {
            DragItem slot = mainItemBar.GetChild(e.from.slotIndex).GetComponentInChildren<DragItem>();
            slot.transform.SetParent(mainItemBar.GetChild(e.to.slotIndex));
            slot.transform.SetAsFirstSibling();
            slot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else if(e.from.containerIndex == 0)
        {
            RemoveItem(mainItemBar, new PositionArgs(e.from));
        }
        else if (e.to.containerIndex == 0)
        {
            ItemStats item = EquipmentManager.instance.GetItemStatsValue(e.to);
            NewItemUI(mainItemBar, new CreateItemArgs(item,e.to,false));
        }   
    }
    private void RemoveDragItemUI(object sender, EventArgs e)
    {
        Transform slot = itemParent.GetComponentInChildren<DragItem>().transform;
        slot.gameObject.SetActive(false);
        Destroy(slot.gameObject);
    }
    private void UpdateDragItemCount(object sender, ItemCountArgs e)
    {
        Transform slot = itemParent.GetComponentInChildren<DragItem>().transform;
        if (e.count != 1) slot.GetChild(0).GetComponent<TextMeshProUGUI>().text = e.count.ToString();
        else slot.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
    }
    private void UpdateItemCount(object sender, UpdateItemCountArgs e)
    {
        Transform grid = GetGrid(e.position.containerIndex);
        UpdateCount(grid, e);
        if(e.position.containerIndex == 0)
        {
            UpdateCount(mainItemBar,e);
        }    
    }
    private void UpdateCount(Transform gridUI, UpdateItemCountArgs e)
    {
        Transform slot = gridUI.GetChild(e.position.slotIndex).GetComponentInChildren<DragItem>().transform;
        if (e.count != 1) slot.GetChild(0).GetComponent<TextMeshProUGUI>().text = e.count.ToString();
        else slot.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
    }
    private void RemoveItemUI(object sender, PositionArgs e)
    {
        Transform grid = GetGrid(e.position.containerIndex);
        if(EquipmentManager.instance.HasPlaceholders(e.position))
        {
            SwitchPlaceholder(true,grid.GetChild(e.position.slotIndex));
        }

        RemoveItem(grid, e);
        if(e.position.containerIndex == 0)
        {
            RemoveItem(mainItemBar, e);
        }
    }
    private void RemoveItem(Transform gridUI, PositionArgs e)
    {
        Transform slot = gridUI.GetChild(e.position.slotIndex).GetComponentInChildren<DragItem>().transform;
        slot.gameObject.SetActive(false);
        Destroy(slot.gameObject);
    }
    private void SwitchPlaceholder(bool turnOn,Transform parentSlot)
    {
        for (int i = 0; i < parentSlot.childCount; i++)
        {
            if(parentSlot.GetChild(i).CompareTag("Placeholder"))
            {
                if(turnOn)
                {
                    parentSlot.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    parentSlot.GetChild(i).gameObject.SetActive(false);
                }
                return;
            }
        }
    }
    private Transform GetGrid(int gridIndex)
    {
        switch(gridIndex)
        {
            case 2: return equipmentClothes;
        }
        return null;
    }
    private void MoveItemUI(object sender, MoveItemUIArgs e)
    {
        Transform gridFrom = GetGrid(e.from.containerIndex);
        Transform gridTo = GetGrid(e.to.containerIndex);
        
        if(EquipmentManager.instance.HasPlaceholders(e.to))
        {
            SwitchPlaceholder(false, gridTo.GetChild(e.to.slotIndex));
        }
      

        if (e.to.containerIndex == 0 || e.from.containerIndex == 0) MoveMainBarItem(this, new MoveItemArgs(e.from, e.to));


        DragItem slot = gridFrom.GetChild(e.from.slotIndex).GetComponentInChildren<DragItem>();
        slot.transform.SetParent(gridTo.GetChild(e.to.slotIndex));
        slot.transform.SetAsFirstSibling();
        slot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        slot.IsInSlot();           
    }
    private void CreateItemUI(object sender, CreateItemArgs e)
    {
        Transform gridUI = GetGrid(e.position.containerIndex);
        if (EquipmentManager.instance.HasPlaceholders(e.position))
        {
            SwitchPlaceholder(false,gridUI.GetChild(e.position.slotIndex));
        }

        NewItemUI(gridUI, e);
        if(e.position.containerIndex == 0 && !e.isDrag)
        {
            NewItemUI(mainItemBar, e);
        }
    }

    private void SetBarColor(Transform bar,ItemStats itemStats)
    {
        Image image = bar.GetComponent<Image>();
        image.color = GetColorForItem(itemStats.itemID);
    }


    public Color GetColorForItem(int itemID)
    {
        Type type = ItemsAsset.instance.GetType(itemID);
        Color color = Color.hotPink;
        foreach (var item in colors)
        {
            if (type == item.type)
            {
                color = item.color;
                break;
            }
        }
        return color;
    }

    public Property GetProperty(string name)
    {
        if(properties.TryGetValue(name, out Property p))
            return p;
        return null;
    }

    public string GetColorHexStringForItem(int itemID)
    {
        return UnityEngine.ColorUtility.ToHtmlStringRGB(GetColorForItem(itemID));
    }



    private void NewItemUI(Transform gridUI, CreateItemArgs e)
    {
        RectTransform transform = Instantiate(item, gridUI.GetChild(e.position.slotIndex)).GetComponent<RectTransform>();
        transform.SetAsFirstSibling();

        if (e.itemStats as IBarValue != null)
        {
            Transform bar = Instantiate(itembar, transform).transform.GetChild(0);
            SetBarColor(bar, e.itemStats);
            UpdateBar((e.itemStats as IBarValue).GetBarValue(),bar);
        }

        transform.anchoredPosition = Vector2.zero;

        transform.GetComponent<Image>().sprite = ItemsAsset.instance.GetIcon(e.itemStats.itemID);
        if (e.itemStats.quantity != 1) transform.GetComponentInChildren<TextMeshProUGUI>().text = e.itemStats.quantity.ToString();
        else transform.GetComponentInChildren<TextMeshProUGUI>().text = "";

        if (gridUI != mainItemBar)
        {
         //   transform.GetComponent<DragDrop>().SetCanvas(mainCanvas);
          //  transform.GetComponent<DragDrop>().IsInSlot();
        }
        else
        {
            transform.GetComponent<DragItem>().enabled = false;
            transform.GetComponent<ItemSlotTooltipTrigger>().enabled = false;
        }

    }






    private void SetColor(ItemStats stats,Transform itemTransform)
    {
        Image image = itemTransform.GetComponent<Image>();
        if(image == null) return;

        if(stats.color.HasValue)
        {
            Material mat = new Material(UIAssetsManager.instance.UIColorItem);
            image.material = mat;
            mat.SetColor("_Color",stats.color.Value);
            image.SetMaterialDirty();
        }
        else
            image.material = null;         
    }

    private bool SetBarValue(ItemStats stats, Transform itemObj)
    {
        if (stats as IBarValue != null)
        {
            EQBar obj = itemObj.GetComponentInChildren<EQBar>(true);
            Transform bar;
            if (obj == null)
                bar = Instantiate(itembar, itemObj).transform.GetChild(0);
            else
                bar = obj.transform.GetChild(0);

            SetBarColor(bar, stats);
            UpdateBar((stats as IBarValue).GetBarValue(), bar);
            return true;
        }
        return false;
    }
    private void SetWetness(float value,Transform itemTransform)
    {
        value = math.clamp(value, 0f, 1f);
        EQWetness obj;
        obj = itemTransform.GetComponentInChildren<EQWetness>(true);

        if (value >= 0.0001)
        {
            obj.gameObject.SetActive(true);
            obj.gameObject.GetComponentInChildren<EQWetnessFill>(true).GetComponent<Image>().fillAmount = value;
        }
        else
            obj.gameObject.SetActive(false);
    }
    private void SetQuality(Quality quality,Transform itemTransform)
    {
        EQQuality obj;
        obj = itemTransform.GetComponentInChildren<EQQuality>(true);
        if(quality == Quality.none || quality == Quality.normal)
            obj.gameObject.SetActive(false);
        else
        {
            obj.gameObject.SetActive(true);
            obj.gameObject.GetComponent<Image>().sprite = UIAssetsManager.instance.GetQualitySprite(quality);
        }
    }




    private void NewItemUI(Transform gridUI, ItemStats itemStats, int slotIndex)
    {
        RectTransform transform = Instantiate(item, gridUI.GetChild(slotIndex)).GetComponent<RectTransform>();
        transform.SetAsFirstSibling();

        SetBarValue(itemStats, transform);
        SetWetness(itemStats.GetFloatWetness(), transform);
        SetColor(itemStats,transform);
        SetQuality(itemStats.quality, transform);



        transform.anchoredPosition = Vector2.zero;
        transform.GetComponent<Image>().sprite = ItemsAsset.instance.GetIcon(itemStats.itemID);

        if (itemStats.quantity != 1) transform.GetComponentInChildren<TextMeshProUGUI>().text = itemStats.quantity.ToString();
        else transform.GetComponentInChildren<TextMeshProUGUI>().text = "";


        if (gridUI != mainItemBar)
        {
         //   transform.GetComponent<DragDrop>().SetCanvas(mainCanvas);
            transform.GetComponent<DragItem>().IsInSlot();
        }
        else
        {
            transform.GetComponent<DragItem>().enabled = false;
            transform.GetComponent<ItemSlotTooltipTrigger>().enabled = false;
        }

    }
    public void SwitchBackground(bool value)
    {
        background.gameObject.SetActive(value);
    }

    public void OpenEquipment(bool value)
    {
        background.gameObject.SetActive(value);
        tabUI.SetActive(value);
        tabUI.equipmentTab.TurnTab(value);
    }

           
    private void ResetSelectedItem()
    {
        if(itemParent.childCount > 0)
        { 
            Transform obj = itemParent.GetChild(0);
            obj.GetComponent<DragItem>().ResetItem();
        }
    }
       
    private RectTransform lastSlotUI;
    private RectTransform currentSlotUI;
    private void UpdateButtonSize()
    {
        // if(lastSlotUI != null && lastSlotUI.localScale.x != buttonScale)
        // {
        //     float scale = math.lerp(lastSlotUI.localScale.x, buttonScale,Time.deltaTime * speedUnselecting);
        //     lastSlotUI.localScale = new Vector3(scale, scale,1);
        // }

        // if (currentSlotUI != null && currentSlotUI.sizeDelta.x != selectedButtonScale)
        // {
        //     float scale = math.lerp(currentSlotUI.localScale.x, selectedButtonScale, Time.deltaTime * speedSelecting);
        //     currentSlotUI.localScale = new Vector3(scale, scale,1);
        // }
    }


    private void LoadClothesSlots(EquipmentGrid grid)
    {
        for (int i = 0; i < grid.gridTransform.childCount; i++)
        {
            grid.gridTransform.GetChild(i).GetComponent<DropSlot>().SetSlotPosition(i, grid.gridIndex);
        }
    }

    public void UpdateDragItem(Transform dragDrop,ItemStats stats)
    {
        if (dragDrop == null || stats == null) return;
        dragDrop.GetComponent<Image>().sprite = ItemsAsset.instance.GetIcon(stats.itemID);
        dragDrop.GetComponentInChildren<TextMeshProUGUI>().text = stats.quantity > 1 ? stats.quantity.ToString() : "";
        SetWetness(stats.GetFloatWetness(),dragDrop);
        SetColor(stats,dragDrop);
        SetQuality(stats.quality,dragDrop);
        SetBarValue(stats, dragDrop);
    }

    public RectTransform CreateDragItem()
    {
        var dragItem = Instantiate(item).GetComponent<DragItem>();
        dragItem.SlotSelected();
        return dragItem.GetComponent<RectTransform>();
    }

    public void UpdateItemSlot(Container container,ItemStats slot, int slotIndex, bool mainBar = false)
    {
        Transform slotObj;
        Transform parent = container.gridTransform;

        if (mainBar)
        {
            slotObj = mainItemBar.GetChild(slotIndex);
            parent = mainItemBar;
        }
        else
        {
            slotObj = container.gridTransform.GetChild(slotIndex);
            if (container.gridIndex == 0) UpdateItemSlot(container, slot, slotIndex, true);
        }




        if (slot == null || slot.quantity == 0)
        {
            if (slotObj.childCount > 0 && slotObj.GetComponentInChildren<DragItem>() != null)
            {
                Destroy(slotObj.GetChild(0).gameObject);
                if (container.mandatoryProperties != MandatoryProperties.none)
                    slotObj.GetComponentInChildren<EQPlaceholder>(true)?.gameObject.SetActive(true);
            }
        }
        else
        {
            if (slotObj.childCount > 0 && slotObj.GetComponentInChildren<DragItem>() != null)
            {
                var item = slotObj.GetChild(0);
                item.GetComponent<Image>().sprite = ItemsAsset.instance.GetIcon(slot.itemID);
                item.GetComponentInChildren<TextMeshProUGUI>().text = slot.quantity > 1 ? slot.quantity.ToString() : "";
                SetWetness(slot.GetFloatWetness(), item);
                SetQuality(slot.quality, item);
                SetColor(slot,item);
                SetBarValue(slot, item);
            }
            else
            {
                NewItemUI(parent, slot, slotIndex);
                if (container.mandatoryProperties != MandatoryProperties.none)
                    slotObj.GetComponentInChildren<EQPlaceholder>(true)?.gameObject.SetActive(false);
            }
        }
    }

    public void TurnOnItemPlaceholder(Container container,int slotIndex)
    {
        Transform slotObj = container.gridTransform.GetChild(slotIndex);

        if (container.mandatoryProperties != MandatoryProperties.none)
            slotObj.GetComponentInChildren<EQPlaceholder>(true).gameObject.SetActive(true);
    }



    private Sprite LoadPlaceholder(MandatoryProperties mandatoryProperties, int mandatoryData)
    {
        Sprite icon = null;
        if (mandatoryProperties == MandatoryProperties.tag)
            icon = ItemsAsset.instance.GetTagIcon(mandatoryData);
        else if (mandatoryProperties == MandatoryProperties.item)
            icon = ItemsAsset.instance.GetIcon(mandatoryData);
        return icon;
    }
    private Sprite LoadPlaceholder(Container container)
    {
        return LoadPlaceholder(container.mandatoryProperties, container.mandatoryData);
    }

    public EquipmentGrid LoadBarSlots(Container containerComponent, Entity entity)
    {
        EquipmentGrid equipmentGrid = new EquipmentGrid(mainItemBar, 0);
        LoadSlots(equipmentGrid,entity, containerComponent, true);
        return equipmentGrid;
    }
    
    private Transform GetItem(SlotPosition position)
    {
        Transform parent = null;

        if(parent != null)
        {
           return parent.GetChild(position.slotIndex).GetComponentInChildren<DragItem>().transform;
        }
        else
        {
            return null;
        }

    }
    private Transform GetItemFromMainBar(SlotPosition position)
    {

        if (position.slotIndex < mainItemBar.childCount) return mainItemBar.GetChild(position.slotIndex).GetComponentInChildren<DragItem>().transform;
        else return null;
    }

    Dictionary<int, Transform> itemRecipes;
    private void LoadRecipes()
    { 
        ReadOnlyCollection<Item> items = ItemsAsset.instance.GetRecipesCrafTable(-1);
        itemRecipes = new Dictionary<int, Transform>();

        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            Transform recipe = Instantiate(itemSlot, recipes).transform;
            recipe.name = item.ID.ToString();
            Transform icon = Instantiate(recipeIcon, recipe).transform;
            icon.GetComponent<Image>().sprite = item.icon;
            if(item.numberItem != 1) icon.GetComponentInChildren<TextMeshProUGUI>().text = item.numberItem.ToString();
            icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            recipe.AddComponent<Recipe>().SetID(item.ID);
            itemRecipes.Add(item.ID, recipe);
        }
    }

    string lastValue = string.Empty;
    int selectedID = -1;
    private bool CheckLastValue(string value)
    {
        return  (value.Length == 0 && lastValue.Length == 0) || 
            (lastValue.Length > 0 && value.Length >= lastValue.Length && lastValue.ToLower() == value.Substring(0, lastValue.Length).ToLower());
    }
    public void Search(string value)
    {
        if(CheckLastValue(value))
        {
            if (value.Length == lastValue.Length) return;
            for (int i = 0; i < recipes.childCount; i++)
            {
                Transform child = recipes.GetChild(i);
                if(child.gameObject.activeSelf)
                {
                    ComperName(child, value,int.Parse(child.name));
                }
            }
            lastValue = value;
            return;
        }

        for (int i = 0; i < recipes.childCount; i++)
        {
            Transform child = recipes.GetChild(i);
            ComperName(child, value, int.Parse(child.name));
        }
        lastValue = value;
    }
    private void ComperName(Transform child,string searchText,int itemID)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        if (item.name.Length >= searchText.Length && item.name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
        {
            child.gameObject.SetActive(true);
        }
        else
        {
            child.gameObject.SetActive(false);
        }
    }
    private void CheckRecipes()
    {
        // Dictionary<int,int> items = EquipmentManager.instance.GetItemDictionary();
        // foreach (var item in itemRecipes)
        // {
        //     CheckRecipe(item.Key, items);
        // }
    }
    private void CheckRecipe(int id, Dictionary<int, int> items)
    {
        Transform child = itemRecipes[id];
        Image backgroundItem = child.GetComponent<Image>();
        if (CanCraft(int.Parse(child.name),items))
        {
            if (backgroundItem.color == Color.white) return;
            backgroundItem.color = Color.white;
            child.GetChild(0).GetComponent<Image>().color = Color.white;
           // child.SetAsFirstSibling();
        }
        else
        {
            if (backgroundItem.color != Color.white) return;
            backgroundItem.color = new Color(1, 1, 1, 0.5f);
            child.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
           // child.SetAsLastSibling();
        }
    }
    private bool CanCraft(int ID,Dictionary<int,int> items)
    {
        Item item = ItemsAsset.instance.GetItem(ID);
        for (int i = 0; i < item.crafingIngredients.Length; i++)
        {
            items.TryGetValue(item.crafingIngredients[i].itemID, out int value);
            if (value < item.crafingIngredients[i].number) return false;
        }
        return true;
    }
    private bool CanCraft(int ID)
    {
       return CanCraft(ID, EquipmentManager.instance.GetItemDictionary());
    }
    public void Craft(int id)
    {
        if (selectedID != id)
        {
            SelectItem(id);
            return;
        }


        if (CanCraft(id))
        {
            EquipmentManager.instance.Craft(id);
            Sounds.instance.Click();
            Dictionary<int, int> items = EquipmentManager.instance.GetItemDictionary();
            CheckRecipe(id, items);      
        }
    }
    public void LoadIngredients(int id)
    {
        Item item = ItemsAsset.instance.GetItem(id);
        if (item != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (item.crafingIngredients.Length > i)
                {
                    Transform child = ingredients.GetChild(i);
                    Ingredient crafingIngredient = item.crafingIngredients[i];
                    Item ingredient = ItemsAsset.instance.GetItem(crafingIngredient.itemID);
                    child.gameObject.SetActive(true);
                    child.GetChild(0).GetComponent<Image>().sprite = ingredient.icon;
                    child.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = crafingIngredient.number.ToString();
                    child.GetChild(1).GetComponent<TextMeshProUGUI>().text = ingredient.name;
                }
                else
                {
                    ingredients.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }

    private void SelectItem(int id)
    {
        if (id >= 0)
        {
            if (itemRecipes.ContainsKey(selectedID)) itemRecipes[selectedID].GetComponent<Image>().sprite = unSelected;
            selectedID = id;
            LoadIngredients(id);
            itemRecipes[id].GetComponent<Image>().sprite = selected;
        }
        else
        {
            if (itemRecipes.ContainsKey(selectedID)) itemRecipes[selectedID].GetComponent<Image>().sprite = unSelected;
            selectedID = id;
            ClearIngredients();
        }
    }
    public void ClearIngredients()
    {
        selectedID = -1;
        for (int i = 0; i < 5; i++)
        {
            ingredients.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void CheckRecipesWithItem(int id,bool increasedItemCount)
    {
        // if (equipment.gameObject.activeSelf)
        // {
        //     int counter = EquipmentManager.instance.CountItems(id);
        //     ReadOnlyCollection<Item> items = ItemsAsset.instance.GetRecipesCrafTable(-1);
        //     Dictionary<int, int> eq = EquipmentManager.instance.GetItemDictionary();
        //     foreach (var item in items)
        //     {
        //         foreach (var ingredient in item.crafingIngredients)
        //         {
        //             if (UpdateCheck(item.ID,ingredient, counter, id, eq, increasedItemCount)) break;
        //         }
        //     }
        // }
    }




    private bool UpdateCheck(int idRecipe, Ingredient ingredient,int counter,int id, Dictionary<int, int> eq,bool increasedItemCount)
    {
        if (ingredient.itemID == id)
        {
            if (increasedItemCount && ingredient.number <= counter)
            {
                CheckRecipe(idRecipe, eq);
                return true;
            }
            else if(!increasedItemCount && ingredient.number > counter)
            {
                CheckRecipe(idRecipe, eq);
                return true;
            }         
        }
        return false;
    }

    Timer timer;

    public void ButtonIsHolding(bool value,int id)
    {
        if(value)
        {
            k = 0;
            timer = Timer.Create(0.2f, () =>
            {
                SelectItem(id);
                isHold = true;
                return true;
            });
        }
        else
        {
            isHold = false;
            timer.Cancel();
        }
    }

    public Timer[] collectedItemTimers = new Timer[5];
    public Transform[] notices = new Transform[5];

    private void SetUpNotices()
    {
        for (int i = 0; i < 5; i++)
        {
            notices[i] = collectedItems.GetChild(i);
        }
    }





    public void NewCollectedItem(int id,int count)
    {
        for (int i = 0; i < 5; i++)
        {
            if (collectedItemTimers[i] == null)
            {
                CreateNotice(id, count, i);
                return;
            } 
        }

        int index = 0;
        float min = collectedItemTimers[0].GetTime();
        for (int i = 1; i < 5; i++)
        {
            float time = collectedItemTimers[i].GetTime();
            if (time < min)
            {
                index = i;
                min = time;
            }
        }
        collectedItemTimers[index].Cancel();
        CreateNotice(id,count, index);
    }
    private void CreateNotice(int itemID,int count,int index)
    {
        Transform item = notices[index];
        CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        item.gameObject.SetActive(true);
        item.SetAsLastSibling();
        SetCollectItem(itemID,count, item);
        collectedItemTimers[index] = Timer.Create(1f, () =>
        {
            collectedItemTimers[index] = Timer.Create(() =>
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, Time.deltaTime * 7f);
                if (canvasGroup.alpha < 0.1f)
                {
                    return true;
                }
                return false;
            },
            () =>
            {
                collectedItemTimers[index] = null;
                item.gameObject.SetActive(false);
                return true;
            });
            return true;
        });
    }
    private void SetCollectItem(int itemID, int itemCount , Transform obj)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        obj.GetChild(0).GetComponent<Image>().sprite = item.icon;
        obj.GetChild(1).GetComponent<TextMeshProUGUI>().text = UIStringsHelper.GetColorfulString(itemCount == 1 ? "" : itemCount.ToString(),GamePreferences.instance.highlightColorStr)  + " " +item.name;
    }



    #region  Containers Funcs

    public Transform CreateUIContainer(ContainerType type)
    {
        ContainerUI containerUI = containers.Find((ContainerUI k) => { return k.containerType == type;});
        return Instantiate(containerUI.prefab,containerUI.parent).transform;
    } 
    public void LoadSlots(EquipmentGrid equipmentGrid,Entity entity, Container containerComponent,bool numbering = false)
    {
        OpenEquipment(true);
        bool isMainBar = equipmentGrid.gridTransform == mainItemBar;
        Sprite icon = LoadPlaceholder(containerComponent.mandatoryProperties, containerComponent.mandatoryData);

        if (numbering)
        {
            int index;
            for (int i = containerComponent.itemSlots.Length - 1; i >= 0; i--)
            {
                index = i + 1;
                Transform slot = Instantiate(itemSlot, equipmentGrid.gridTransform).transform;
                if (!isMainBar)
                {
                    slot.AddComponent<DropSlot>().SetSlotPosition(i, equipmentGrid.gridIndex);
                    slot.AddComponent<ItemSlotTooltipTrigger>();
                }
                else
                    Destroy(slot.GetComponent<Button>());

                slot.SetAsFirstSibling();
                Instantiate(slotIndex, slot).GetComponent<TextMeshProUGUI>().text = (index % 10).ToString();
            }
        }
        else
        {
            for (int i = 0; i < containerComponent.itemSlots.Length; i++)
            {
                Transform slot = Instantiate(itemSlot, equipmentGrid.gridTransform).transform;
                if (icon != null)
                {
                    Transform grey = Instantiate(greyIcon, slot).transform;
                    grey.GetComponent<Image>().sprite = icon;
                }
                slot.AddComponent<DropSlot>().SetSlotPosition(i, equipmentGrid.gridIndex);
                slot.AddComponent<ItemSlotTooltipTrigger>();
            }

            equipmentGrid.gridTransform.GetComponentInChildren<EQOptionsTag>(true)?.transform.SetAsLastSibling();
        }

        var slots = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(entity);
        var bars = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<ItemBarData>(entity);

        for (int i = 0; i < containerComponent.itemSlots.Length; i++)
        {
            var item = containerComponent.itemSlots[i];
            if (item == null) continue;
            
            if (icon != null) equipmentGrid.gridTransform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            NewItemUI(equipmentGrid.gridTransform, item, i);
        }

        if (!isMainBar)
        {
            LoadContainerOptions(equipmentGrid,containerComponent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(equipmentGrid.gridTransform.parent.parent.GetComponent<RectTransform>());
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(equipmentGrid.gridTransform.parent.GetComponent<RectTransform>());
        OpenEquipment(false);
    }
    private void LoadContainerOptions(EquipmentGrid equipmentGrid, Container containerComponent)
    {
        Transform options = equipmentGrid.gridTransform.GetComponentInChildren<EQOptionsTag>(true)?.transform;
        if(options == null) return;

        Instantiate(containerInfo, options).AddComponent<StaticTooltipTrigger>().SetUp(containerComponent);
    }
    
    #endregion

    #region  Slot In Hand Funcs
    Timer itemInHandPopupTimer;

    public void UpdateSlotInHand((int lastSlot,int newSlot) slotIndex,InventorySlot? itemSlot)
    {
        if(itemInHandPopupTimer != null)
            itemInHandPopupTimer.Cancel();


        Transform last = mainItemBar.GetChild(slotIndex.lastSlot);
        last.GetComponent<Image>().sprite = unSelected;
        Sounds.instance.Click();

        if(itemSlot.HasValue && ItemsAsset.instance.TryGetItem(itemSlot.Value.itemId,out var item))
        {
            itemInHandPopup.gameObject.SetActive(true);
            itemInHandPopup.text = item.name;
            var canvasGroup = itemInHandPopup.GetComponent<CanvasGroup>();

            itemInHandPopupTimer = Timer.Create(0.3f,() =>
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, Time.deltaTime * 7f);
                if (canvasGroup.alpha < 0.1f)
                {
                    return true;
                }
                return false;
            },() =>
            {
                canvasGroup.alpha = 1;
                canvasGroup.gameObject.SetActive(false);
            });

        }
        else
        {
            itemInHandPopup.gameObject.SetActive(false);
        }
        

        Transform current = mainItemBar.GetChild(slotIndex.newSlot);
        current.GetComponent<Image>().sprite = selected;
    }
    
    public void NewItemInHand(InventorySlot? itemSlot,InventorySlot[] ammoToSelect, int selectedAmmoIndex,InventorySlot[] magazine)
    {
        if(itemSlot.HasValue && ItemsAsset.instance.TryGetItem(itemSlot.Value.itemId,out var item))
            UpdateAmmoInfo(item,ammoToSelect,selectedAmmoIndex,magazine);
        else
            UpdateAmmoInfo(null,null,-1,null);
    }




    public void UpdateAmmoInfo(Item item,InventorySlot[] ammoList,int selectedAmmoIndex,InventorySlot[] magazine)
    {
        int ammoCount = ammoList  != null ? ammoList.Length : 0;
        int magazineCount = magazine  != null ? magazine.Length : 0;
        RangedWeapon rangedWeapon = item as RangedWeapon;

        Debug.Log("update UI" + magazineCount);

        for(int i = ammoCountersParent.childCount - 1; i >= ammoCount; i--)
            Destroy(ammoCountersParent.GetChild(i).gameObject);

        if(rangedWeapon != null)
        {   
            for(int i = ammoMagazineParent.childCount - 1; i >= rangedWeapon.magazineCapacity; i--)
                Destroy(ammoMagazineParent.GetChild(i).gameObject);
            
            for(int i = 0; i < ammoList.Length;i++)
            {
                var ammo = ammoList[i];
                if(ItemsAsset.instance.TryGetItem(ammo.itemId, out var itemAsset))
                {
                    GameObject icon = GetNextUIElement(ammoCounerPrefab,ammoCountersParent,i);           
                    icon.transform.GetChild(0).GetComponentInChildren<Image>().sprite = itemAsset.icon;
                    icon.GetComponentInChildren<TextMeshProUGUI>().text = ammo.quantity.ToString();
                }
            }
      
            if(rangedWeapon.hasMagazine)
            {
                int j;
                for(j = 0;j < magazineCount;j++)
                {
                    var element = magazine[j];
                    if(ItemsAsset.instance.TryGetItem<Ammo>(element.itemId, out var itemAsset))
                    {
                        GameObject icon = GetNextUIElement(ammoMagazinePrefab,ammoMagazineParent,j);  
                        icon.GetComponent<Image>().sprite = itemAsset.UIBulletIcon;
                    }
                }
                
                int free = rangedWeapon.magazineCapacity - magazineCount;
                if(free > 0)
                {
                    for(int k = 0; k < free; k++)
                    {
                        GameObject icon = GetNextUIElement(ammoMagazinePrefab,ammoMagazineParent,j+k);
                               Debug.Log("index + " + (j+k).ToString() + " " + icon);

                        icon.GetComponent<Image>().sprite = rangedWeapon.ammoTag.NoAmmoIconUI;        
                    }
                }
            }

            UpdateSelectedAmmo(selectedAmmoIndex);           
        }
        else
        {
            for(int i = ammoMagazineParent.childCount - 1; i >= 0; i--)
                Destroy(ammoMagazineParent.GetChild(i).gameObject);
        }
    }

    private GameObject GetNextUIElement(GameObject prefab,Transform parent, int index)
    {
        if(index >= parent.childCount)
            return Instantiate(prefab,parent);
        else
            return parent.GetChild(index).gameObject;                  
    }


    public void UpdateSelectedAmmo(int childIndex)
    {
        Sounds.instance.Click();
        for(int i = ammoCountersParent.childCount - 1; i >= 0; i--)
        {
           var icon = ammoCountersParent.GetChild(i).gameObject;
            if(childIndex == i)
                icon.transform.GetComponent<Image>().sprite = UIAssetsManager.instance.selectedIronBackgroundUI;
            else
                icon.transform.GetComponent<Image>().sprite = UIAssetsManager.instance.ironBackgroundUI;
        }
    }
    #endregion




}



