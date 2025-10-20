using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;


[System.Serializable]
public class Container
{
    public Entity entity;
    public int gridIndex;
    public Transform gridTransform;
    public ItemStats[] itemSlots;
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
        }
    }

    #endregion

    #region Slot Management
    #region Selected Slot Management
    
    public ItemStats SelectItem(SlotPosition slotPosition, SelectionMode selectionMode, int n = 1)
    {
        ItemStats item = GetItemSlot(slotPosition);
        n = SelectN(item.quantity, selectionMode, n);
        selectedItem = TakeItems(slotPosition, n);
        selectedSlot = slotPosition;
        RPCHelper.SendRpc(ClientServerBootstrap.ClientWorld.EntityManager, new EQSelectItem() { position = slotPosition, value = n });
        return selectedItem;
    }
    public void DeselectItem()
    {
        selectedItem = null;
        selectedSlot = SlotPosition.NullSlot;
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
    private ItemStats TakeItems(SlotPosition slotPosition,int number)
    {
        ItemStats itemSlot = GetItemSlot(slotPosition);
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
        return GetItemSlot(slotPosition) == null;
    }
    public void MoveItemData(SlotPosition to, int quantity)
    {
        if (SlotIsEmpty(to) || selectedItem.itemID == GetItemSlot(to).itemID)
        {
            quantity = Math.Clamp(quantity, 0, selectedItem.quantity);
            bool itemExist = SetOrAddItemSlot(to, new ItemStats(selectedItem.itemID, quantity));

            selectedItem.quantity -= quantity;
            SendMoveItem(to, quantity);
            LocalUpdateSlotIndex(to);

            Debug.Log("<color=blue> " + selectedItem.quantity);
            DragManager.instance.UpdateSelected(selectedItem);
            if (selectedItem.quantity == 0)
                DeselectItem();
        }
    }
    public void MoveItemData(SlotPosition to, SelectionMode selectionMode, int n = 1)
    {
        n = SelectN(selectedItem.quantity, selectionMode, n);
        MoveItemData(to, n);
    }




    private ItemStats GetItemSlot(SlotPosition slotPosition)
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
        if (GetItemSlot(slotPosition) != null)
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
        Container container = containers[slotPosition.containerIndex];
        var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.entity);
        foreach (var item in buffer)
        {
            if (item.slot == slotPosition.slotIndex)
            {
                ItemStats slot = new ItemStats(item);
                
                Debug.Log("<Color=red> " + slot.quantity);
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
            UIManager.instance.UpdateItemSlot(container, GetItemSlot(slotPosition), slotPosition.slotIndex);
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
