using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


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
        Debug.Log("<Color=red>" + selectedItem + " " + (selectedItem is DestroyableItem));


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
        Debug.Log("<Color=red>" + selectedItem + " " + (selectedItem is DestroyableItem));



        if (SlotIsEmpty(slotPosition))
            UIManager.instance.TurnOnItemPlaceholder(containers[slotPosition.containerIndex], slotPosition.slotIndex);
        LocalUpdateSlotIndex(slotPosition);
        return selectedItem;
    }

    public void ClearSelection()
    {
        selectedItem = null;
        Debug.Log("<Color=red>" + selectedItem + " " + (selectedItem is DestroyableItem));

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
    public void DeselectItem(ref EntityCommandBuffer ecb)
    {
        if (selectedItem != null)
        {
            RPCHelper.SendRpc(ref ecb, new EQDeselectItem() { });
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


        Debug.Log(selectedItem is DestroyableItem);

        bool itemExist = SetOrAddItemSlot(to, selectedItem.Clon(quantity));
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

    public bool CanMove(SlotPosition to, out bool haveSameId)
    {
        haveSameId = false;
        if (selectedItem != null && containers.TryGetValue(to.containerIndex, out var container))
        {
            if (container.itemSlots.Length > to.slotIndex && container.itemSlots[to.slotIndex] != null) 
                haveSameId = selectedItem.itemID == container.itemSlots[to.slotIndex].itemID;
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
        Debug.Log(itemSlot + " ------ " + (itemSlot is DestroyableItem));
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
        var bars = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<ItemBarData>(entity);
        foreach (InventorySlot slot in slots)
        {
            ItemBarData? itemBarData = null;
            foreach (ItemBarData barData in bars)
            {
                if(barData.slot == slot.slot)
                {
                    itemBarData = barData;
                    break;
                }
            }

            container.itemSlots[slot.slot] = CreateItemStats(slot, itemBarData);
        }


        UIManager.instance.LoadSlots(v, entity, container, v.gridIndex == 0);
        if (containerComponent.containerIndex == 0)
        {
            UIManager.instance.LoadBarSlots(container, entity);
            // ChangeSelectedSlot(0);
        }
    }
    private ItemStats LoadItemFromEntities(SlotPosition slotPosition)
    {
        Container container = containers[slotPosition.containerIndex];
        var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.entity);
        var bars = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<ItemBarData>(container.entity);


        foreach (var item in buffer)
        {
            if (item.slot == slotPosition.slotIndex )
            {
                ItemBarData? itemBarData = null;
                foreach (ItemBarData barData in bars)
                {
                    if (barData.slot == item.slot)
                    {
                        itemBarData = barData;
                        break;
                    }
                }
                ItemStats slot = CreateItemStats(item, itemBarData);
                Debug.Log("  " + (slot is DestroyableItem).ToString());

                if (slotPosition.slotIndex >= 0) container.itemSlots[slotPosition.slotIndex] = slot;
                return slot;
            }
        }




        if (slotPosition.slotIndex >= 0) container.itemSlots[slotPosition.slotIndex] = null;
        return null;
    }

    private ItemStats LoadSelectedItemFromEntities(int containerIndex)
    {
        Container container = containers[containerIndex];
        var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.entity);
        var bars = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<ItemBarData>(container.entity);


        foreach (var item in buffer)
        {
            if (item.slot < 0)
            {
                ItemBarData? itemBarData = null;
                foreach (ItemBarData barData in bars)
                {
                    if (barData.slot == item.slot)
                    {
                        itemBarData = barData;
                        break;
                    }
                }
                ItemStats slot = CreateItemStats(item, itemBarData);
                return slot;
            }
        }

        return null;
    }

    private ItemStats CreateItemStats(InventorySlot slot,ItemBarData? itemBarData)
    {
        if (itemBarData.HasValue)
            return new DestroyableItem(slot, itemBarData.Value);
        else
            return new ItemStats(slot);
    }

    public void UpdateSlotIndex(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container))
        {
            bool s = false;
            if (!selectedSlot.Compare(SlotPosition.NullSlot) && slotPosition.Compare(selectedSlot))
            {
                ItemStats item = LoadItemFromEntities(new SlotPosition(selectedSlot.containerIndex,
                    EQHelper.ConvetSlotIndexToSelectedSlotIndex(selectedSlot.slotIndex)));

                selectedItem = item;
                Debug.Log("<Color=red>" + selectedItem + " " + (selectedItem is DestroyableItem));

                s = true;
                DragManager.instance.UpdateSelected(item);
            }
            else
            {
                ItemStats item = LoadSelectedItemFromEntities(slotPosition.containerIndex);
                if (item != null)
                {
                    selectedItem = item;
                    Debug.Log("<Color=red>" + selectedItem + " " + (selectedItem is DestroyableItem));

                    selectedSlot = slotPosition;
                    DragManager.instance.UpdateSelected(item);
                }
            }

           // Debug.Log("Update!!! " + slotPosition.ToString() );
            ItemStats itemSlot = LoadItemFromEntities(slotPosition);
           // Debug.Log("TOo " + itemSlot);
            UIManager.instance.UpdateItemSlot(container, itemSlot, slotPosition.slotIndex);
        }
    }
    public void ClearContainer(int index, ref EntityCommandBuffer ecb)
    {
        if (containers.TryGetValue(index,out Container container))
        {
            for (int i = 0;i < container.itemSlots.Length;i++)
            {
                var item = container.itemSlots[i];
                if (item != null)
                {
                    container.itemSlots[i] = null;
                    UIManager.instance.UpdateItemSlot(container, null, i);
                }
            }

            if (selectedSlot.containerIndex == index)
                DeselectItem(ref ecb);
        }
    }
    public void ClearAllContainers(ref EntityCommandBuffer ecb)
    {
        Debug.Log("dziala!");
        foreach (var container in containers.Values)
        {   
            for (int i = 0; i < container.itemSlots.Length; i++)
            {
                var item = container.itemSlots[i];
                if (item != null)
                {
                    container.itemSlots[i] = null;
                    UIManager.instance.UpdateItemSlot(container, null, i);
                }
            }
            if (selectedSlot.containerIndex == container.gridIndex)
                DeselectItem(ref ecb);
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
