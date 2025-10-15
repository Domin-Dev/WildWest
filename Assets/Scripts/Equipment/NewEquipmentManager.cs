using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;


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

    private Dictionary<int, Container> containers = new Dictionary<int, Container>();
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
        int number = n;
        switch (selectionMode)
        {
            case SelectionMode.TakeHalf:
                number = item.quantity/2;
                break;
            case SelectionMode.TakeAll:
                number = item.quantity;
                break;
        }
        selectedItem = TakeItems(slotPosition, number);
        selectedSlot = slotPosition;
        return selectedItem;
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
    public void MoveItemData(SlotPosition to)
    {
        if (SlotIsEmpty(to))
        {
            SetItemSlot(to,selectedItem);
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            RPCHelper.SendRpc(ref entityCommandBuffer, new EQMoveItem() { from = selectedSlot, to = to , value = selectedItem.quantity});
            entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
            entityCommandBuffer.Dispose();
        }
        else
        {

        }
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
        Debug.Log(ClientServerBootstrap.ClientWorld.EntityManager.GetComponentData<ContainerComponent>(container.entity).containerIndex);
        var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.entity);
        foreach (var item in buffer)
        {
            Debug.Log(item.ItemId + " " + item.slot);
            if (item.slot == slotPosition.slotIndex)
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
            Debug.Log(slotPosition.containerIndex);
            ItemStats itemSlot = LoadItemFromEntities(slotPosition);
            Debug.Log(itemSlot?.ToString() + ' ' + slotPosition);
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
    #endregion
}
