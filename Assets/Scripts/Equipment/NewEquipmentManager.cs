using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


[System.Serializable]
public class Container
{
    public Entity entity;
    public int gridIndex;
    public Transform gridTransform;
    public ItemStats[] itemSlots;
    public MandatoryProperties mandatoryProperties;
    public int mandatoryData;
}
public enum SelectionMode
{
    N,
    All,
    Half,
}
public class NewEquipmentManager : MonoBehaviour
{
    #region Variables
    public static NewEquipmentManager instance {  get; private set; }

    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containerParent;

    public Dictionary<int, Container> containers = new Dictionary<int, Container>();
    private bool open = false;

    private ItemStats selectedItem;
    private SlotPosition selectedSlot;

    #endregion

    #region Monobehaviour Functions
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Update()
    {
        if (!ChatManager.instance.isChatting)
        {
            if (InputManager.i.equipment.triggered)
            {
                open = !open;
                UIManager.instance.OpenEquipment(open);
            }

            if (Input.GetMouseButton(0))
            {
                if(selectedItem != null && UIManager.instance.mouseIsOverEQUI)
                {
                   // Debug.Log("Drop!!!");
                }
            }
        }
    }

    #endregion

    #region Selected Slot Management
    
    public ItemStats SelectItem(SlotPosition slotPosition, SelectionMode selectionMode, int n = 1)
    {
        ItemStats item = GetItemStats(slotPosition);
        n = SelectN(item.quantity, selectionMode, n);
        selectedItem = TakeItems(slotPosition, n);
        selectedSlot = slotPosition;
        RPCHelper.SendRpc(ClientServerBootstrap.ClientWorld.EntityManager, new EQSelectItem() { position = slotPosition, value = n });
        if (SlotIsEmpty(slotPosition))
            UIManager.instance.TurnOnItemPlaceholder(containers[slotPosition.containerIndex], slotPosition.slotIndex);
        return selectedItem;
    }
    public ItemStats LocalSelectItem(SlotPosition slotPosition, ItemStats itemStats)
    {
        selectedSlot = slotPosition;
        selectedItem = itemStats;
        if (SlotIsEmpty(slotPosition))
            UIManager.instance.TurnOnItemPlaceholder(containers[slotPosition.containerIndex], slotPosition.slotIndex);
        LocalUpdateSlotIndex(slotPosition);
        return selectedItem;
    }

    public void ClearSelection()
    {
        selectedItem = null;
        selectedSlot = SlotPosition.NullSlot;
    }

    public void DeselectItem()
    {
        if (selectedItem != null)
        {
            RPCHelper.SendRpc(ClientServerBootstrap.ClientWorld.EntityManager, new EQDeselectItem() { });
        }
        ClearSelection();
        DragManager.instance.UpdateSelected(null);
    }

    public void CombineAllItems(SlotPosition slotPosition)
    {
        RPCHelper.SendRpc(ClientServerBootstrap.ClientWorld.EntityManager, new EQCombineAllItems() { position = slotPosition});
    }
    private int SelectN(int itemQuantity, SelectionMode selectionMode, int n = 1)
    {
        switch (selectionMode)
        {
            case SelectionMode.Half:
                n = (int)Math.Ceiling(itemQuantity / 2f);
                break;
            case SelectionMode.All:
                n = itemQuantity;
                break;
        }
        return n;
    }
    #endregion

    #region Slot Management
    private ItemStats TakeItems(SlotPosition slotPosition,int number)
    {
        ItemStats itemSlot = GetItemStats(slotPosition);
        int n = itemSlot.quantity - number;
        if(n > 0)
        { 
            itemSlot.quantity = n;
            return new ItemStats(itemSlot.itemID,number);
        }
        else
        {
            SetItemSlot(slotPosition, null);
            return itemSlot;
        }
    }
    public bool SlotIsEmpty(SlotPosition slotPosition)
    {
        return GetItemStats(slotPosition) == null;
    }
    public ItemStats MoveItemData(SlotPosition to, int quantity)
    {
        int maxStack = ItemsAsset.instance.GetStackMax(selectedItem.itemID);
        ItemStats stats = GetItemStats(to);
        ItemStats statsToReturn = null;

        if (stats != null && (stats.itemID != selectedItem.itemID || stats.quantity >= maxStack))
        {
            statsToReturn = GetItemStats(to);
            stats = null;
            ClearSlot(to);
        }

        quantity = Math.Clamp(quantity, 0, selectedItem.quantity);
        if (stats != null && quantity + stats.quantity > maxStack)
            quantity = maxStack - stats.quantity;      
        bool itemExist = SetOrAddItemSlot(to, new ItemStats(selectedItem.itemID, quantity));
        selectedItem.quantity -= quantity;
        SendMoveItem(to, quantity);
        LocalUpdateSlotIndex(to);

        DragManager.instance.UpdateSelected(selectedItem);
        if (selectedItem.quantity == 0)
            ClearSelection();

        return statsToReturn;
    }
    public ItemStats MoveItemData(SlotPosition to, SelectionMode selectionMode, int n = 1)
    {
        n = SelectN(selectedItem.quantity, selectionMode, n);
        return MoveItemData(to, n);
    }

    public bool CanMove(SlotPosition to)
    {

        if (selectedItem != null && containers.TryGetValue(to.containerIndex, out var container))
        {
           return EQHelper.CheckRequirements(container.mandatoryProperties, container.mandatoryData, selectedItem.itemID);
        }
        return false;
    }
    private ItemStats GetItemStats(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container) && container.itemSlots.Length > slotPosition.slotIndex)
        {
            return container.itemSlots[slotPosition.slotIndex];
        }
        return null;
    }
    private void SetItemSlot(SlotPosition slotPosition, ItemStats itemSlot)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container) && container.itemSlots.Length > slotPosition.slotIndex)
        {
            container.itemSlots[slotPosition.slotIndex] = itemSlot;
        }
    }
    private void AddItemSlot(SlotPosition slotPosition, ItemStats itemSlot)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container) && container.itemSlots.Length > slotPosition.slotIndex)
        {
            container.itemSlots[slotPosition.slotIndex].quantity += itemSlot.quantity;
        }
    }

    // Return true if exist itemslot
    private bool SetOrAddItemSlot(SlotPosition slotPosition, ItemStats itemSlot)
    {
        if (GetItemStats(slotPosition) != null)
        {
            AddItemSlot(slotPosition, itemSlot);
            return true;
        }
        SetItemSlot(slotPosition, itemSlot);
        return false;
    }
    private void ClearSlot(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container) && container.itemSlots.Length > slotPosition.slotIndex)
        {
            container.itemSlots[slotPosition.slotIndex] = null;
        }
    }
    #endregion

    #region Slot Synchronization
    public void LoadContainer(ContainerComponent containerComponent, Entity entity)
    {
        if (containers.ContainsKey(containerComponent.containerIndex)) return;
        Container container = new Container();

        container.gridTransform = Instantiate(containerPrefab, containerParent).transform;
        container.entity = entity;
        container.gridIndex = containerComponent.containerIndex;
        container.itemSlots = new ItemStats[containerComponent.capacity];
        container.mandatoryProperties = containerComponent.mandatoryProperties;
        container.mandatoryData = containerComponent.mandatoryData;

        var v = new EquipmentGrid(container.gridTransform, containerComponent.containerIndex);
        containers.Add(containerComponent.containerIndex, container);

        var slots = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(entity);
        foreach (InventorySlot slot in slots)
        {
            container.itemSlots[slot.slot] = new ItemStats(slot);
        }


        UIManager.instance.LoadSlots(v, entity, containerComponent, v.gridIndex == 0);
        if (containerComponent.containerIndex == 0)
        {
            UIManager.instance.LoadBarSlots(containerComponent, entity);
            // ChangeSelectedSlot(0);
        }
    }
    private ItemStats LoadItemFromEntities(SlotPosition slotPosition)
    {
        if(slotPosition.slotIndex < 0) return null;

        Container container = containers[slotPosition.containerIndex];
        var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.entity);
        foreach (var item in buffer)
        {
            if (item.slot == slotPosition.slotIndex )
            {
                ItemStats slot = new ItemStats(item);
                container.itemSlots[slotPosition.slotIndex] = slot;
                return slot;
            }
        }

        container.itemSlots[slotPosition.slotIndex] = null;
        return null;
    }
    public void UpdateSlotIndex(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container))
        {
            ItemStats itemSlot = LoadItemFromEntities(slotPosition);
            UIManager.instance.UpdateItemSlot(container, itemSlot, slotPosition.slotIndex);
        }
    }
    public void LocalUpdateSlotIndex(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container))
        {
            UIManager.instance.UpdateItemSlot(container, GetItemStats(slotPosition), slotPosition.slotIndex);
        }
    }
    private void SendMoveItem(SlotPosition to, int quantity)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        RPCHelper.SendRpc(ref entityCommandBuffer, new EQMoveItem() { to = to, value = quantity });
        entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }

    #endregion
}
