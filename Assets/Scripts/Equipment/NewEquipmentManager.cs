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
    TakeN,
    TakeAll,
    TakeHalf,
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
        switch (selectionMode)
        {
            case SelectionMode.TakeHalf:
                n = (int)Math.Ceiling(item.quantity/2f);
                break;
            case SelectionMode.TakeAll:
                n = item.quantity;
                break;
        }
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
    public bool MoveItemData(SlotPosition to)
    {
        if (to.Compare(selectedSlot))
        {
            SetOrAddItemSlot(to, selectedItem);
            LocalUpdateSlotIndex(to);
            DeselectItem();
            return true;
        }

        if (SlotIsEmpty(to) || selectedItem.itemID == GetItemSlot(to).itemID)
        {
            bool itemExist = SetOrAddItemSlot(to, selectedItem);

            if (!selectedSlot.Compare(to))
            {
                SendMoveItem(to);
            }
            LocalUpdateSlotIndex(to);
            DeselectItem();
            return itemExist;
        }


        return false;
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
                ItemStats current = container.itemSlots[slotPosition.slotIndex];

                Debug.Log("<Color=red> " + slot.quantity);
                if(!slotPosition.Compare(selectedSlot))
                    container.itemSlots[slotPosition.slotIndex] = slot;
                else if(selectedItem.itemID == slot.itemID)
                {
                    if (slot.quantity >= current?.quantity + selectedItem.quantity)
                    {
                        slot.quantity -= selectedItem.quantity;
                        if(slot.quantity >= 0)
                            container.itemSlots[slotPosition.slotIndex] = slot;
                    }
                    else if (slot.quantity < selectedItem.quantity)
                    {
                        selectedItem.quantity = slot.quantity;
                        DragManager.instance.UpdateSelected(slot);
                    }
                    else
                    {
                        slot.quantity -= selectedItem.quantity;
                        container.itemSlots[slotPosition.slotIndex] = slot;
                    }
                }

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
    private void SendMoveItem(SlotPosition to)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        RPCHelper.SendRpc(ref entityCommandBuffer, new EQMoveItem() { to = to, value = selectedItem.quantity });
        entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }

    #endregion
}
