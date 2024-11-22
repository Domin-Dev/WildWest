using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public struct SlotPosition
{
    public int gridIndex;
    public int slotIndex;

    public SlotPosition(int gridIndex, int slotIndex)
    {
        this.gridIndex = gridIndex;
        this.slotIndex = slotIndex;
    }

    public bool Compare(SlotPosition slotPosition)
    {
        if (slotPosition.gridIndex == gridIndex && slotPosition.slotIndex == slotIndex) return true;
        else return false;
    }

    public static SlotPosition NullSlot = new SlotPosition(-1, -1);

    public override string ToString()
    {
        return $"Grid Index:{gridIndex},Slot Index:{slotIndex}";
    }

}
public class TooltipInfo
{
    public string content;
    public string header;
    public TooltipInfo(string content, string header)
    {
        this.content = content;
        this.header = header;
    }

    public TooltipInfo(string content)
    {
        this.content = content;
        this.header = "";
    }
}
public class UpdateSelectedSlotInBarArgs : EventArgs
{
    public int lastSlot;
    public int currentSlot;
    public UpdateSelectedSlotInBarArgs(int lastSlot, int currentSlot)
    {
        this.lastSlot = lastSlot;
        this.currentSlot = currentSlot;
    }
}
public class BoolArgs : EventArgs
{
    public bool value;
    public BoolArgs(bool value)
    {
        this.value = value;
    }
}
public class CreateItemArgs : EventArgs
{
    public ItemStats itemStats;
    public SlotPosition position;
    public bool isDrag;
    public CreateItemArgs(ItemStats itemStats, SlotPosition slotPosition, bool isDrag)
    {
        this.isDrag = isDrag;
        this.itemStats = itemStats;
        this.position = slotPosition;
    }
}
public class MoveItemUIArgs : EventArgs
{
    public SlotPosition from;
    public SlotPosition to;

    public MoveItemUIArgs(SlotPosition from, SlotPosition to)
    {
        this.from = from;
        this.to = to;
    }
}
public class PositionArgs : EventArgs
{
    public SlotPosition position;

    public PositionArgs(SlotPosition position)
    {
        this.position = position;
    }
}
public class UpdateItemCountArgs : PositionArgs
{
    public int count;

    public UpdateItemCountArgs(SlotPosition position, int count) : base(position)
    {
        this.count = count;
    }
}
public class ItemCountArgs : EventArgs
{
    public int count;
    public ItemCountArgs(int count)
    {
        this.count = count;
    }
}
public class MoveItemArgs : EventArgs
{
    public SlotPosition from;
    public SlotPosition to;
    public MoveItemArgs(SlotPosition from, SlotPosition to)
    {
        this.from = from;
        this.to = to;
    }
}
public class ItemStatsArgs : EventArgs
{
    public ItemStats item;

    public ItemStatsArgs(ItemStats item)
    {
        this.item = item;
    }
}
public class LifeBarArgs : PositionArgs
{
    public float barValue;
    public LifeBarArgs(SlotPosition slotPosition, float barValue) : base(slotPosition)
    {
        this.barValue = barValue;
    }
}
public class SetAmmoBarArgs : EventArgs
{
    public int magazineCapacity;
    public int currentCount;
    public AmmoType type;

    public SetAmmoBarArgs(int magazineCapacity, int currentCount, AmmoType ammoType)
    {
        this.magazineCapacity = magazineCapacity;
        this.currentCount = currentCount;
        this.type = ammoType;
    }
}
public class UpdateAmmoBarArgs : EventArgs
{
    public int currentCount;

    public UpdateAmmoBarArgs(int currentCount)
    {
        this.currentCount = currentCount;
    }
}

public class PlaceholderArgs : EventArgs
{
    public bool turn;
    public SlotPosition slotPosition;

    public PlaceholderArgs(bool turn, SlotPosition slotPosition)
    {
        this.turn = turn;
        this.slotPosition = slotPosition;
    }
}

public class EquipmentManager : MonoBehaviour
{
    public event EventHandler<UpdateSelectedSlotInBarArgs> UpdateSelectedSlotInBar;
    public event EventHandler<BoolArgs> OpenEquipmentUI;

    public event EventHandler<CreateItemArgs> CreateItemUI;
    public event EventHandler<MoveItemUIArgs> MoveItemUI;
    public event EventHandler<PositionArgs> RemoveItemUI;
    public event EventHandler<UpdateItemCountArgs> UpdateItemCount;

    public event EventHandler<ItemCountArgs> UpdateDragItemCount;
    public event EventHandler RemoveDragItemUI;

    public event EventHandler<MoveItemArgs> MoveMainBarItem;
    public event EventHandler<CreateItemArgs> CreateMainBarItem;
    public event EventHandler<PositionArgs> RemoveMainBarItem;
    public event EventHandler<UpdateItemCountArgs> UpdateMainBarItemCount;

    public event EventHandler<PlaceholderArgs> TurnPlaceholder;

    public event EventHandler<LifeBarArgs> UpdateItemLifeBar;

    public event EventHandler<ItemStatsArgs> UpdateItemInHand;

    private int slotInHand { get; set; } = 1;
    private int ammoCount;
    private int ammoID;

    private bool equipmentIsOpen = false;

    public static readonly int BarSlotCount = 10;
    public static readonly int SlotCount = 30;
    public static readonly int clothesCount = 8;

    public static readonly GarmentType[] clothesSlots =
    {
        GarmentType.headwear,
        GarmentType.faceCover,
        GarmentType.outerwear,
        GarmentType.shirt,
        GarmentType.pants,
        GarmentType.belt,
        GarmentType.accessory,
        GarmentType.bag
    };

    private ItemStats[] equipment = new ItemStats[SlotCount];
    private ItemStats[] equipmentBar = new ItemStats[BarSlotCount];
    private ItemStats[] clothes = new ItemStats[clothesCount];
    private ItemStats[] container;

    private Dictionary<int,int> dedicatedGrids = new Dictionary<int,int>();

    private SlotPosition selectedSlotInEQ;
    private ItemStats selectedItemStats;
    private List<int> placeholderGrids = new List<int>();

    [HideInInspector] public PointerEventData.InputButton input;
    public static EquipmentManager instance { private set; get; }

    [SerializeField] private CharacterSpriteController player;

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
        UIManager.instance.SetUpUIEquipment(this);
        selectedSlotInEQ = new SlotPosition(-1, -1);
        placeholderGrids.Add(2);

        ChangeSelectedSlot(0);
    }

    private void Update()
    {
        if (!ChatManager.instance.isChatting)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeSelectedSlot(4);
            else if (Input.GetKeyDown(KeyCode.Alpha6)) ChangeSelectedSlot(5);
            else if (Input.GetKeyDown(KeyCode.Alpha7)) ChangeSelectedSlot(6);
            else if (Input.GetKeyDown(KeyCode.Alpha8)) ChangeSelectedSlot(7);
            else if (Input.GetKeyDown(KeyCode.Alpha9)) ChangeSelectedSlot(8);
            else if (Input.GetKeyDown(KeyCode.Alpha0)) ChangeSelectedSlot(9);


            if (Input.GetKeyDown(KeyCode.I))
            {
                equipmentIsOpen = !equipmentIsOpen;
                BoolArgs args = new BoolArgs(equipmentIsOpen);
                container = null;
                OpenEquipmentUI(this, args);
            }

            if (Input.mouseScrollDelta.y > 0)
            {
                NextSlot();
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                BackSlot();
            }
        }
    }

    public void SetUpEvent(HandsController handsController)
    {
        handsController.UseItem += UseSelectedItem;
        BuildingManager.instance.builtObject += BuiltObject;
    }

    public void MoveUpItem(SlotPosition slotPosition)
    {
        if (container != null)
        {
            if (slotPosition.gridIndex == 3)            
                FindGoodSlot(slotPosition,new int[]{0,1});
            else if (slotPosition.gridIndex == 2)
                FindGoodSlot(slotPosition, new int[] {3,0,1});
            else
                FindGoodSlot(slotPosition,new int[]{3});
        }
        else
        {
            if (slotPosition.gridIndex == 0)
                FindGoodSlot(slotPosition, new int[] {1});
            else if (slotPosition.gridIndex == 2)
                FindGoodSlot(slotPosition, new int[] {0,1});
            else
                FindGoodSlot(slotPosition, new int[] { 0});
        }
    }

    public void MoveUpItems(SlotPosition slotPosition)
    {
        if (slotPosition.gridIndex == 2) return;
        int id = GetItemStats(slotPosition).itemID;  
        if (container != null)
        {
            if (slotPosition.gridIndex == 3)
            {
                FindGoodSlots(id,new int[]{3},new int[] {0,1});
            }
            else
            {
                FindGoodSlots(id, new int[] {0,1}, new int[] {3});
            }
        }
        else
        {
            if (slotPosition.gridIndex == 0)
            {
                FindGoodSlots(id,0,1);
            }
            else
            {
                FindGoodSlots(id,1,0);
            }
        }

    }

    private void FindGoodSlots(int itemID, int from, int to)
    {
        FindGoodSlots(itemID,new int[] { from }, new int[] { to } );
    }

    private void FindGoodSlots(int itemID, int[] gridsFrom, int[] gridsTo)
    {
        var items = FindItems(itemID, gridsFrom);
        CheckDedicatedGrids(ref gridsTo,itemID);
        if (gridsTo.Length == 0) return;
        for (int i = 0; i < items.Count; i++)
        {
            FindGoodSlot(items[i], gridsTo);
        }
    }
    private bool FindGoodSlot(SlotPosition slotPosition, int[] grids)
    {
        ItemStats itemStats = GetItemStats(slotPosition);
        int stackMax = itemStats.GetMaxStack();
        CheckDedicatedGrids(ref grids,itemStats.itemID);
        var itemList = FindItems(itemStats.itemID, grids);
        if (itemList.Count > 0)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                ItemStats item = GetItemStats(itemList[i]);
                int free = stackMax - item.itemCount;

                if (free > 0)
                {
                    if (itemStats.itemCount > free)
                    {
                        itemStats.itemCount -= free;
                        IncreaseItemCount(itemList[i], free);
                    }
                    else
                    {
                        if (HasPlaceholders(slotPosition))
                        {
                            TurnPlaceholder(this, new PlaceholderArgs(true, slotPosition));
                        }
                        IncreaseItemCount(itemList[i], itemStats.itemCount);
                        ClearSlot(slotPosition);
                        RemoveItemUI(this, new PositionArgs(slotPosition)); 
                        return true;
                    }
                }
            }
            UpdateCount(slotPosition);
        }

        for (int i = 0; i < grids.Length; i++)
        {
            int x = FindFreeSlot(GetGrid(grids[i]));
            if(x >= 0)
            {
                MoveItem(slotPosition, new SlotPosition(grids[i], x));
                UpdateItemInHand(this, new ItemStatsArgs(GetItemStats(new SlotPosition(0, slotInHand))));
                if (HasPlaceholders(slotPosition))
                {
                    TurnPlaceholder(this, new PlaceholderArgs(true, slotPosition));
                    if (slotPosition.gridIndex == 2)
                    {
                        Garment garment = (Garment)ItemsAsset.instance.GetItem(itemStats.itemID);
                        player.RemoveClothes((int)garment.type);
                    }
                }
                return true;
            }
        }
        return false;
    }

    private void MoveItem(SlotPosition from, SlotPosition to)
    {
        if(IsFreeSlot(to))
        {
            SetItemStats(to,GetItemStats(from));
            ClearSlot(from);
        }
        else
        {
            ItemStats x = GetItemStats(to);
            SetItemStats(to,GetItemStats(from));
            SetItemStats(from,x);
        }
        MoveItemUI(this,new MoveItemUIArgs(from, to));
        
    }

    private int FindFreeSlot(ItemStats[] items)
    {
        for (int i = 0; i < items.Length; i++)
        { 
            if (items[i] == null)
            {
                return i;
            }
        }
        return -1;
    }
    public void LoadChest(GridContainer gridContainer)
    {
        equipmentIsOpen = true;
        container = gridContainer.items;
        ContainerItem cont = (ContainerItem)ItemsAsset.instance.GetItem(gridContainer.ID);
        if (cont.ItemContainer.itemID < 0)
        {
            placeholderGrids.Remove(3);
            dedicatedGrids.Remove(3);
            UIManager.instance.LoadSlotsContainer(container);
        }
        else
        {
            placeholderGrids.Add(3);
            dedicatedGrids.TryAdd(3, cont.ItemContainer.itemID);

            Sprite icon = ItemsAsset.instance.GetIcon(cont.ItemContainer.itemID);
            UIManager.instance.LoadSlotsDedicatedContainer(container, icon);
        }

    }
    public bool HasPlaceholders(int gridIndex)
    {
        return placeholderGrids.Contains(gridIndex);
    }
    public bool HasPlaceholders(SlotPosition slotPosition)
    {
        return HasPlaceholders(slotPosition.gridIndex);
    }
    private void BuiltObject(object sender, EventArgs e)
    {
        if (DecreaseItemCount(new SlotPosition(0, slotInHand), 1) <= 0)
            UpdateItemInHand(this, new ItemStatsArgs(equipmentBar[slotInHand]));
    }


    private void UseSelectedItem(object sender, EventArgs e)
    {
        DestroyableItem item = equipmentBar[slotInHand] as DestroyableItem;
        if (item != null)
        {
            item.Use();
            UpdateItemLifeBar(this, new LifeBarArgs(new SlotPosition(0, slotInHand), item.GetLifePointsInPercent()));
        }
    }
    public void UnselectedSlot()
    {
        ItemStats itemStats = GetItemStats(selectedSlotInEQ);
        if (itemStats == null || itemStats.itemID == selectedItemStats.itemID)
        {
            if (itemStats != null)
            {
                PutItems(selectedItemStats, SlotPosition.NullSlot, selectedSlotInEQ);
                if (selectedSlotInEQ.gridIndex == 0) UpdateMainItemCount(selectedSlotInEQ, GetItemStats(selectedSlotInEQ).itemCount);
            }
            else
            {
                SetItemStats(selectedSlotInEQ, selectedItemStats);
                if (selectedSlotInEQ.gridIndex == 0) NewMainBarItemUI(GetItemStats(selectedSlotInEQ), selectedSlotInEQ);
                NewItemUI(selectedItemStats, selectedSlotInEQ, true);
            }
        }
        else
        {
            AddNewItem(selectedItemStats);
        }

        UpdateCount(selectedSlotInEQ);
        RemoveDragItem();
        ClearSelectedSlot();
    }
    public void ClearSelectedSlot()
    {
        selectedSlotInEQ = new SlotPosition(-1, -1);
        selectedItemStats = null;
    }
    public void SelectedSlotTakeAll(SlotPosition slotPosition)
    {
        if (HasPlaceholders(slotPosition)) TurnPlaceholder(this, new PlaceholderArgs(true, slotPosition));
        selectedSlotInEQ = slotPosition;
        selectedItemStats = GetItemStats(slotPosition);
        ClearSlot(slotPosition);
        if (slotPosition.gridIndex == 0) RemoveMainBarItemUI(slotPosition);
    }
    public void SelectedSlotTakeHalf(SlotPosition slotPosition)
    {
        ItemStats itemStats = GetItemStats(slotPosition);
        selectedSlotInEQ = slotPosition;
        if (itemStats.itemCount == 1)
        {
            SelectedSlotTakeAll(slotPosition);
            return;
        }

        int half = itemStats.itemCount / 2;
        selectedItemStats = new ItemStats(itemStats.itemID, itemStats.itemCount - half);
        itemStats.itemCount = half;

        NewItemUI(itemStats, selectedSlotInEQ, true);
        UpdateCount(selectedSlotInEQ);
        UpdateDragCount(selectedItemStats.itemCount);
    }
    private void NextSlot()
    {
        if (slotInHand == BarSlotCount - 1)
        {
            ChangeSelectedSlot(0);
        }
        else
        {
            ChangeSelectedSlot(slotInHand + 1);
        }
    }
    private void BackSlot()
    {
        if (slotInHand == 0)
        {
            ChangeSelectedSlot(BarSlotCount - 1);
        }
        else
        {
            ChangeSelectedSlot(slotInHand - 1);
        }
    }
    private void ChangeSelectedSlot(int newSlot)
    {
        newSlot = math.clamp(newSlot, 0, BarSlotCount - 1);
        if (slotInHand != newSlot)
        {
            UpdateSelectedSlotInBarArgs args = new UpdateSelectedSlotInBarArgs(slotInHand, newSlot);
            UpdateSelectedSlotInBar(this, args);
            slotInHand = newSlot;
            UpdateItemInHand(this, new ItemStatsArgs(equipmentBar[newSlot]));
        }
    }

    private bool CheckTab(ItemStats[] items,int gridIndex,ItemStats itemStats,int stackMax)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                if (itemStats.itemCount > stackMax)
                {
                    itemStats.itemCount -= stackMax;
                    ItemStats newItem = itemStats.Clon();
                    newItem.itemCount = stackMax;

                    items[i] = newItem;
                    NewItemUI(newItem, new SlotPosition(gridIndex, i), false);
                }
                else
                {
                    items[i] = itemStats;
                    NewItemUI(itemStats, new SlotPosition(gridIndex, i), false);
                    UIManager.instance.CheckRecipesWithItem(itemStats.itemID, true);
                    UIManager.instance.NewCollectedItem(itemStats);
                    return true;
                }
            }
        }
        return false;
    }
    public bool AddNewItem(ItemStats itemStats)
    {
        if(itemStats == null) return false;
        if (itemStats.itemCount > 0)
        {
            List<SlotPosition> itemList = FindItems(itemStats.itemID,true);
            int stackMax = ItemsAsset.instance.GetStackMax(itemStats.itemID);

            if (itemList.Count > 0)
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    ItemStats item = GetItemStats(itemList[i]);
                    int free = stackMax - item.itemCount;
                    if (free > 0)
                    {
                        if (itemStats.itemCount > free)
                        {
                            itemStats.itemCount -= free;
                            IncreaseItemCount(itemList[i], free);
                        }
                        else
                        {
                            IncreaseItemCount(itemList[i], itemStats.itemCount);
                            UIManager.instance.CheckRecipesWithItem(itemStats.itemID, true);
                            UIManager.instance.NewCollectedItem(itemStats);
                            return true;
                        }
                    }
                }
            }

            if(CheckTab(equipmentBar, 0, itemStats, stackMax)) return true;
            if(CheckTab(equipment, 1, itemStats, stackMax)) return true;

        }
        return false;
    }
    public void NewItemUI(ItemStats itemStats, SlotPosition slotPosition, bool isDrag)
    {
        if (slotPosition.Compare(new SlotPosition(0, slotInHand)))
        {
            UpdateItemInHand(this, new ItemStatsArgs(equipmentBar[slotInHand]));
        }

        CreateItemArgs args = new CreateItemArgs(itemStats, slotPosition, isDrag);
        CreateItemUI(this, args);
    }
    public void NewMainBarItemUI(ItemStats itemStats, SlotPosition slotPosition)
    {
        CreateItemArgs args = new CreateItemArgs(itemStats, slotPosition, false);
        CreateMainBarItem(this, args);
    }
    public bool IsFreeSlot(SlotPosition position)
    {
        var grid = GetGrid(position.gridIndex);
        if (grid != null)
        {
            return grid[position.slotIndex] == null;
        }
        return false;
    }
    private bool IsSlotInHand(SlotPosition slotPosition)
    {
        return slotPosition.Compare(new SlotPosition(0, slotInHand));
    }

    private void CheckDedicatedGrids(ref int[] grids,int idItem)
    {
        List<int> newGrid = new List<int>();
        for (int i = 0; i < grids.Length; i++)
        {
            if(!CheckDedicatedGrid(grids[i], idItem))
            {
                newGrid.Add(grids[i]);
            }
        }
        grids = newGrid.ToArray();
        Debug.Log(grids.Length);
    }
    private bool CheckDedicatedGrid(SlotPosition target)
    {
        return CheckDedicatedGrid(target.gridIndex,selectedItemStats.itemID);
    }
    private bool CheckDedicatedGrid(int gridIndex ,int idItem)
    {
        if (dedicatedGrids.ContainsKey(gridIndex))
        {
            if (idItem != dedicatedGrids[gridIndex])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


    public void MoveSelectedItem(SlotPosition target)
    {
        if (CheckDedicatedGrid(target) || (target.gridIndex == 2 && !IsGarment(target, selectedItemStats)) || (selectedSlotInEQ.gridIndex == 2 && !IsTheType(target)))
        {
            UnselectedSlot();
            MoveSelectedItemEnd(target);
            return;
        }

        if (IsFreeSlot(target))
        {
            SetItemStats(target, selectedItemStats);
            if (target.gridIndex == 0)
            {
                NewMainBarItemUI(selectedItemStats, target);
            }
            else if (HasPlaceholders(target))
            {
                TurnPlaceholder(this, new PlaceholderArgs(false, target));
            }
        }
        else if (!target.Compare(selectedSlotInEQ))
        {
            ItemStats itemStatsTarget = GetItemStats(target);
            int maxStack = ItemsAsset.instance.GetStackMax(itemStatsTarget.itemID);
            if (selectedItemStats.itemID != itemStatsTarget.itemID || maxStack == 1)
            {
                if (IsFreeSlot(selectedSlotInEQ))
                {
                    SetItemStats(target, selectedItemStats);
                    SetItemStats(selectedSlotInEQ, itemStatsTarget);
                    MoveItemUIArgs moveItemUIArgs = new MoveItemUIArgs(target, selectedSlotInEQ);
                    MoveItemUI(this, moveItemUIArgs);
                    if (target.gridIndex == 0)
                    {
                        NewMainBarItemUI(selectedItemStats, target);
                    }
                }
                else
                {
                    UnselectedSlot();
                }
            }
            else
            {
                PutItems(selectedItemStats, new SlotPosition(-1, -1), target);
                RemoveDragItem();
            }
        }
        else
        {
            if (!IsFreeSlot(selectedSlotInEQ))
            {
                PutItems(selectedItemStats, SlotPosition.NullSlot, target);
                RemoveDragItem();
            }
            else SetItemStats(selectedSlotInEQ, selectedItemStats);

            UpdateCount(target);
        }

        MoveSelectedItemEnd(target);
    }
    private bool IsGarment(SlotPosition target, ItemStats itemStats)
    {
        if (itemStats == null) return false;
        if (ItemsAsset.instance.CheckItemType<Garment>(itemStats.itemID))
        {
            Garment garment = (Garment)ItemsAsset.instance.GetItem(itemStats.itemID);
            if (garment.type == clothesSlots[target.slotIndex])
            {
                return true;
            }
        }
        return false;
    }

    private bool IsTheType(SlotPosition slotPosition)
    {
        ItemStats itemStats = GetItemStats(slotPosition);
        if(itemStats == null) return true;

        Garment garment = (Garment)ItemsAsset.instance.GetItem(itemStats.itemID);
        if(garment != null && (int)garment.type == selectedSlotInEQ.slotIndex)
        {
            return true;
        }
        return false;
    }

    private void MoveSelectedItemEnd(SlotPosition target)
    {
        if (IsSlotInHand(target) || IsSlotInHand(selectedSlotInEQ))
        {
            UpdateItemInHand(this, new ItemStatsArgs(GetItemStats(new SlotPosition(0, slotInHand))));
        }

        if (IsFreeSlot(selectedSlotInEQ) && HasPlaceholders(selectedSlotInEQ))
        {
            TurnPlaceholder(this, new PlaceholderArgs(true, selectedSlotInEQ));
        }

        ClearSelectedSlot();
    }

    public void PutOneItem(SlotPosition position)
    {
        int stackMax = ItemsAsset.instance.GetStackMax(selectedItemStats.itemID);

        if (selectedItemStats.itemCount > 0)
        {
            if (CheckDedicatedGrid(position)  || (position.gridIndex == 2 && !IsGarment(position, selectedItemStats)) || (selectedSlotInEQ.gridIndex == 2 && !IsTheType(position)))
            {
                UnselectedSlot();
                MoveSelectedItemEnd(position);
                return;
            }


            ItemStats itemStats;
            if (IsFreeSlot(position))
            {
                itemStats = selectedItemStats.Clon();
                itemStats.itemCount = 1;
                SetItemStats(position, itemStats);
                NewItemUI(itemStats, position, false);
            }
            else
            {
                itemStats = GetItemStats(position);
                if (itemStats.itemID != selectedItemStats.itemID || itemStats.itemCount >= stackMax)
                {
                    return;
                }
                itemStats.itemCount += 1;
            }
            selectedItemStats.itemCount--;
            UpdateCount(position);


            if (selectedItemStats.itemCount <= 0)
            {
                RemoveDragItem();
                ClearSelectedSlot();
            }
            else
            {
                UpdateDragCount(selectedItemStats.itemCount);
            }
        }
    }
    private void PutItems(ItemStats item, SlotPosition lastPosition, SlotPosition target)
    {
        ItemStats itemStats = GetItemStats(target);
        if (itemStats == null)
        {
            itemStats = SetItemStats(target, new ItemStats(item.itemID, 0));
            NewItemUI(itemStats, target, false);
        }

        int stackMax = ItemsAsset.instance.GetStackMax(item.itemID);
        int free = stackMax - itemStats.itemCount;

        if (free > 0)
        {
            if (free >= item.itemCount)
            {
                IncreaseItemCount(target, item.itemCount);
            }
            else
            {
                item.itemCount -= free;
                IncreaseItemCount(target, free);
                FindSlotForIt(item, lastPosition);
            }
        }
        else
        {
            FindSlotForIt(item, lastPosition);
        }
    }
    private void FindSlotForIt(ItemStats itemStats, SlotPosition lastPosition)
    {
        if (itemStats != null)
        {
            ItemStats item = GetItemStats(lastPosition);

            if (!lastPosition.Compare(SlotPosition.NullSlot) && ((item != null && item.itemID == itemStats.itemID) || item == null))
            {
                PutItems(itemStats, new SlotPosition(-1, -1), lastPosition);
            }
            else
            {
                AddNewItem(itemStats);
            }
        }
    }

    public void DoubleClick(SlotPosition slotPosition)
    {
        ItemStats itemStats = GetItemStats(slotPosition);
        Item item = ItemsAsset.instance.GetItem(itemStats.itemID);
        if(item is Garment && slotPosition.gridIndex != 2)
        {
            SlotPosition pos = new SlotPosition(2, (int)((Garment)item).type);
            ItemStats a = GetItemStatsValue(pos);
            if (a != null)
            {
                RemoveItem(pos);
            }
            MoveItem(slotPosition,pos);
            ClearSlot(slotPosition);
            if(HasPlaceholders(slotPosition))TurnPlaceholder(this, new PlaceholderArgs(true, slotPosition));
            AddNewItem(a);
        }
        else
        {
            CollectAll(slotPosition);
        }
    }
    public void CollectAll(SlotPosition position)
    {
        ItemStats itemStats = GetItemStats(position);
        if (itemStats != null)
        {
            int stackMax = ItemsAsset.instance.GetStackMax(itemStats.itemID);
            int free = stackMax - itemStats.itemCount;

            if (free <= 0) return;

            List<SlotPosition> items = FindItems(itemStats.itemID,false);
            foreach (SlotPosition itemPosition in items)
            {
                if (!itemPosition.Compare(position))
                {
                    ItemStats item = GetItemStats(itemPosition);
                    if (item.itemCount == stackMax) continue;
                    if (item.itemCount > free)
                    {
                        itemStats.itemCount += free;
                        item.itemCount -= free;
                        UpdateCount(itemPosition);
                        break;
                    }
                    else
                    {
                        itemStats.itemCount += item.itemCount;
                        free -= item.itemCount;
                        ClearSlot(itemPosition);
                        RemoveItem(itemPosition);
                        if (free == 0) break;
                    }
                }
            }
            UpdateCount(position);
        }
    }
    private void ClearSlot(SlotPosition position)
    {
        GetGrid(position.gridIndex)[position.slotIndex] = null;
    }
    private ItemStats[] GetGrid(int gridIndex)
    {
        switch (gridIndex)
        {
            case 0:
                return equipmentBar;
            case 1:
                return equipment;
            case 2:
                return clothes;
            case 3:
                return container;
        }
        return null;
    }
    private ItemStats GetItemStats(SlotPosition position)
    {
        if (position.slotIndex >= 0) return GetGrid(position.gridIndex)[position.slotIndex];
        else return null;
    }
    public ItemStats GetItemStatsValue(SlotPosition position)
    {
        if (position.slotIndex >= 0)
        {
            ItemStats itemStats = GetGrid(position.gridIndex)[position.slotIndex];
            if(itemStats != null) return itemStats.Clon();
        }
        return null;
    }
    private ItemStats SetItemStats(SlotPosition position, ItemStats itemStats)
    {
        if (position.gridIndex == 2)
        {
            Garment garment = (Garment)ItemsAsset.instance.GetItem(itemStats.itemID);
            player.SetClothes((int)garment.type, garment.ID);
        }
        else if (selectedSlotInEQ.gridIndex == 2)
        {
            Garment garment = (Garment)ItemsAsset.instance.GetItem(itemStats.itemID);
            player.RemoveClothes((int)garment.type);
        }

        return GetGrid(position.gridIndex)[position.slotIndex] = itemStats;
    }
    private void UpdateCount(SlotPosition position)
    {
        UpdateItemCountArgs updateItemCountArgs = new UpdateItemCountArgs(position, GetItemStats(position).itemCount);
        UpdateItemCount(this, updateItemCountArgs);
    }
    private void UpdateMainItemCount(SlotPosition position, int count)
    {
        UpdateItemCountArgs updateItemCountArgs = new UpdateItemCountArgs(position, count);
        UpdateMainBarItemCount(this, updateItemCountArgs);
    }
    private void UpdateDragCount(int count)
    {
        ItemCountArgs updateDragItemCountArgs = new ItemCountArgs(count);
        UpdateDragItemCount(this, updateDragItemCountArgs);
    }
    private void RemoveItem(SlotPosition position)
    {
        PositionArgs removeItemUIArgs = new PositionArgs(position);
        RemoveItemUI(this, removeItemUIArgs);
    }
    private void RemoveDragItem()
    {
        RemoveDragItemUI(this, null);
    }
    public bool IsNotSelected()
    {
        return selectedSlotInEQ.Compare(new SlotPosition(-1, -1));
    }
    private List<SlotPosition> FindItems(int ItemId,bool onlyEQ)
    {
        List<SlotPosition> items = new List<SlotPosition>();

        for (int i = 0; i < equipmentBar.Length; i++)
        {
            if (equipmentBar[i] != null && equipmentBar[i].itemID == ItemId)
            {
                items.Add(new SlotPosition(0, i));
            }
        }

        for (int i = 0; i < equipment.Length; i++)
        {
            if (equipment[i] != null && equipment[i].itemID == ItemId)
            {
                items.Add(new SlotPosition(1, i));
            }
        }

        if (!onlyEQ && container != null)
        {
            for (int i = 0; i < container.Length; i++)
            {
                if (container[i] != null && container[i].itemID == ItemId)
                {
                    items.Add(new SlotPosition(3, i));
                }
            }
        }

        return items;
    }

    private List<SlotPosition> FindItems(int ItemId, int[] gridIndexes)
    {
        List<SlotPosition> items = new List<SlotPosition>();

        for (int i = 0; i < gridIndexes.Length; i++)
        {
            int gridIndex = gridIndexes[i];
            ItemStats[] array = GetGrid(gridIndex);     
            for (int j = 0; j < array.Length; j++)
            {
                if (array[j] != null && array[j].itemID == ItemId)
                {
                    items.Add(new SlotPosition(gridIndex, j));
                }
            }
        }

        return items;
    }
    private void RemoveMainBarItemUI(SlotPosition position)
    {
        RemoveMainBarItem(this, new PositionArgs(position));
    }
    private void IncreaseItemCount(SlotPosition position, int value)
    {
        GetItemStats(position).itemCount += value;
        UpdateCount(position);
    }

    private void DecreaseItemCount(int itemID, int value = 1)
    {
        var list = FindItems(itemID, false);
        for (int i = 0; i < list.Count; i++)
        {
            int balance = DecreaseItemCount(list[i], value);
            if (balance < 0) value = -balance;
            else return;
        }
    }
    private int DecreaseItemCount(SlotPosition position, int value = 1)
    {
        ItemStats itemStats = GetItemStats(position);
        int balance = itemStats.itemCount - value;
        if (balance <= 0)
        {
            ClearSlot(position);
            RemoveItem(position);
            UIManager.instance.CheckRecipesWithItem(itemStats.itemID, false);
            return balance;
        }
        itemStats.itemCount = balance;
        UpdateCount(position);
        UIManager.instance.CheckRecipesWithItem(itemStats.itemID, false);
        return 1;
    }
    private SlotPosition Find(int itemId)
    {
        for (int i = 0; i < equipmentBar.Length; i++)
        {
            if (equipmentBar[i] != null && equipmentBar[i].itemID == itemId)
            {
                return new SlotPosition(0, i);
            }
        }

        for (int i = 0; i < equipment.Length; i++)
        {
            if (equipment[i] != null && equipment[i].itemID == itemId)
            {
                return new SlotPosition(1, i);
            }
        }

        return SlotPosition.NullSlot;
    }
    public TooltipInfo GetTooltipInfo(SlotPosition position)
    {
        ItemStats itemStats = GetItemStats(position);
        if (itemStats == null) return null;
        else
        {
            return ItemsAsset.instance.GetTooltipInfo(itemStats.itemID);
        }
    }
    public bool CountAmmo(RangedWeaponItem rangedWeaponItem)
    {
        ammoID = ItemsAsset.instance.GetAmmoID(rangedWeaponItem.itemID);
        List<SlotPosition> ammoList = FindItems(ammoID, true);
        ammoCount = 0;
        for (int i = 0; i < ammoList.Count; i++)
        {
            ammoCount += GetItemStats(ammoList[i]).itemCount;
        }
        return ammoCount > 0;
    }
    public int CountItems(int id)
    {
        var items = FindItems(id, false);
        int counter = 0;
        foreach (var item in items)
        {
            counter += GetItemStats(item).itemCount;
        }
        return counter;
    }

    public int Reload()
    {
        DecreaseItemCount(Find(ammoID));
        ammoCount--;
        return ammoCount;
    }
    public Dictionary<int, int> GetItemDictionary()
    {
        Dictionary<int, int> items = new Dictionary<int, int>();
        AddItemsToDictionary(items, equipment);
        AddItemsToDictionary(items, equipmentBar);
        if(container != null) AddItemsToDictionary(items, container);
        return items;
    }
    private void AddItemsToDictionary(Dictionary<int, int> items, ItemStats[] itemStats)
    {
        foreach (var item in itemStats)
        {
            if (item != null)
            {
                if (items.ContainsKey(item.itemID))
                {
                    items[item.itemID] += item.itemCount;
                }
                else
                {
                    items.Add(item.itemID, item.itemCount);
                }
            }
        }
    }
    public void Craft(int itemID)
    {
        Item item = ItemsAsset.instance.GetItem(itemID);
        ItemStats itemStats = item.GetItemStats();
        itemStats.itemCount = item.numberItem;
        AddNewItem(itemStats);
        for (int i = 0; i < item.crafingIngredients.Length; i++)
        {
            Ingredient ingredient = item.crafingIngredients[i];
            DecreaseItemCount(ingredient.itemID, ingredient.number);
        }
    }

    public void ThrowItem()
    {      
        if(HasPlaceholders(selectedSlotInEQ))
        {
            TurnPlaceholder(this, new PlaceholderArgs(true, selectedSlotInEQ));
        }

        if(selectedSlotInEQ.gridIndex == 2)
        {
            Garment garment = (Garment)ItemsAsset.instance.GetItem(selectedItemStats.itemID);
            player.RemoveClothes((int)garment.type);
        }

        GridVisualization.instance.CreateWorldItem(selectedItemStats, (Vector2)player.transform.position , player.GetThrowDir(UnityEngine.Random.Range(0.25f,0.5f)));  
        if(selectedSlotInEQ.gridIndex == 0 && slotInHand == selectedSlotInEQ.slotIndex)
        {
            UpdateItemInHand(null, new ItemStatsArgs(GetItemStats(selectedSlotInEQ)));
        }
        ClearSelectedSlot();
    }

}
