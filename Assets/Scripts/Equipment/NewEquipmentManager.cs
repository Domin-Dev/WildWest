using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Localization.Settings;


[System.Serializable]
public class Container : IHaveTooltip
{
    public Entity entity;
    public int gridIndex;
    public Transform gridTransform;
    public ItemStats[] itemSlots;
    public MandatoryProperties mandatoryProperties;
    public ContainerType containerType;

    public int mandatoryData;

    public TooltipInfo GetTooltip()
    { 
        StringBuilder header = new StringBuilder();
        StringBuilder content = new StringBuilder();

        header.Append(EquipmentConfig.Instance.GetContainerName(gridIndex));
        content.Append(EquipmentConfig.Instance.GetContainerDescription(gridIndex));

        if(containerType == ContainerType.Standard)
            WriteContainerStats(header,content);
            
           
        return new TooltipInfo(content.ToString(), header.ToString(),null);
    }
    private void WriteContainerStats(StringBuilder header, StringBuilder content)
    {
        if (mandatoryProperties == MandatoryProperties.tag)
        {
            string arg = UIStringsHelper.GetStringWithDefaultColor(ItemsAsset.instance.GetTag(mandatoryData)?.localizedString.GetLocalizedString());
            UIStringsHelper.AppendArgs(content,UIManager.instance.GetProperty("RequiredTag"),arg);
        }
        else if(mandatoryProperties == MandatoryProperties.item)
        {  
            string arg = UIStringsHelper.GetStringWithDefaultColor(ItemsAsset.instance.GetItem(mandatoryData)?.name);
            UIStringsHelper.AppendArgs(content,UIManager.instance.GetProperty("RequiredTag"),arg);
        }


        ContainerComponent c = ClientServerBootstrap.ClientWorld.EntityManager.GetComponentData<ContainerComponent>(entity);
        UIStringsHelper.Append(content, UIManager.instance.GetProperty("ContainerWaterResistance") ,c.waterResistance + " %");
        UIStringsHelper.Append(content,UIManager.instance.GetProperty("Capacity"),c.capacity.ToString());
    }
    
}

public class NewEquipmentManager : MonoBehaviour
{
    #region Variables

    public static NewEquipmentManager instance {  get; private set; }
    public static event Action<(int lastSlot,int newSlot),IReadOnlyItemStats> onNewSlotInHand;

    public Dictionary<int, Container> containers = new Dictionary<int, Container>();
    private bool open = false;
    private ItemStats selectedItem;
    private SlotPosition selectedSlot;
    private int currentSlotInHand;

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

            if(!open)
            {
                
            }
        }
    }
    private void OnEnable()
    {
        PlayerInputSystem.onNewSlotInHand += NewSlotInHand;
    }
    private void OnDisable()
    {
        PlayerInputSystem.onNewSlotInHand -= NewSlotInHand;
    }
    #endregion

    #region Item In Hand Managment

    private void NewSlotInHand(int slotIndex)
    {
        if(!containers.ContainsKey(0)) return;

        Debug.Log("wybrano + " + slotIndex +"  " + currentSlotInHand);
        onNewSlotInHand?.Invoke((currentSlotInHand,slotIndex),containers[0].itemSlots[slotIndex]);
        currentSlotInHand = slotIndex;
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
    public void DeselectItem(ref EntityCommandBuffer ecb)
    {
        if (selectedItem != null)
        {
            RPCHelper.SendRpc(ref ecb, new EQDeselectItem() { });
        }
        ClearSelection();
        DragManager.instance.UpdateSelected(null);
    }
    public void DoubleClick(SlotPosition slotPosition)
    {
        RPCHelper.SendRpc(ClientServerBootstrap.ClientWorld.EntityManager, new EQDoubleClickAction() { position = slotPosition});
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

        if (stats != null && (stats.itemID != selectedItem.itemID || stats.quality != selectedItem.quality || stats.quantity >= maxStack))
        {
            statsToReturn = GetItemStats(to);
            stats = null;
            ClearSlot(to);
        }

        quantity = Math.Clamp(quantity, 0, selectedItem.quantity);
        if (stats != null && quantity + stats.quantity > maxStack)
            quantity = maxStack - stats.quantity;


        bool itemExist = SetOrAddItemSlot(to, selectedItem.Clon(quantity));
        selectedItem.quantity -= quantity;
        SendMoveItem(to, quantity);
        LocalUpdateSlotIndex(to);

        DragManager.instance.UpdateSelected(selectedItem);
        if (TooltipSystem.IsDisplaying(to) && statsToReturn == null)
            TooltipSystem.Show(to, GetItemStats(to),true);

        if (selectedItem.quantity == 0)
            ClearSelection();

        return statsToReturn;
    }
    public ItemStats MoveItemData(SlotPosition to, SelectionMode selectionMode, int n = 1)
    {
        n = SelectN(selectedItem.quantity, selectionMode, n);
        return MoveItemData(to, n);
    }
    public bool CanMove(SlotPosition to, out bool haveSameItem)
    {
        haveSameItem = false;
        if (selectedItem != null && containers.TryGetValue(to.containerIndex, out var container))
        {
            if (container.itemSlots.Length > to.slotIndex && container.itemSlots[to.slotIndex] != null)
            {
                haveSameItem = selectedItem.itemID == container.itemSlots[to.slotIndex].itemID &&
                     selectedItem.quality == container.itemSlots[to.slotIndex].quality;
            }
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
    public ItemStats ReadItemStats(SlotPosition slotPosition)
    {
        var item = GetItemStats(slotPosition);
        if (item == null) return null;
        return item.Clon();
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
            var item =  container.itemSlots[slotPosition.slotIndex];
            item.AddWetness(itemSlot.wetness,itemSlot.quantity);
            item.quantity += itemSlot.quantity;

            ItemWithBar itemWithBarTo = item as ItemWithBar;
            ItemWithBar itemWithBarFrom = itemSlot as ItemWithBar;
            if(itemWithBarFrom != null && itemWithBarTo != null)
                itemWithBarTo.AddBarValue(itemWithBarFrom);
            
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
        if (containers.ContainsKey(containerComponent.containerIndex) || containerComponent.serverContainer) return;
        Container container = new Container();

        container.containerType = EQHelperClient.GetContainerType(containerComponent.containerIndex);
        container.gridTransform = UIManager.instance.CreateUIContainer(container.containerType);
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
            NewSlotInHand(0);
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
            return new ItemWithBar(slot, itemBarData.Value);
        else
            return new ItemStats(slot);
    }
    public void UpdateSlotIndex(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container))
        {
            if (!selectedSlot.Compare(SlotPosition.NullSlot) && slotPosition.Compare(selectedSlot))
            {
                ItemStats item = LoadItemFromEntities(new SlotPosition(selectedSlot.containerIndex,
                    EQHelperClient.ConvetSlotIndexToSelectedSlotIndex(selectedSlot.slotIndex)));
                selectedItem = item;
                DragManager.instance.UpdateSelected(item);
            }
            else
            {
                ItemStats item = LoadSelectedItemFromEntities(slotPosition.containerIndex);
                if (item != null)
                {
                    selectedItem = item;
                    selectedSlot = slotPosition;
                    DragManager.instance.UpdateSelected(item);
                }
            }
            
            ItemStats itemSlot = LoadItemFromEntities(slotPosition);
            UIManager.instance.UpdateItemSlot(container, itemSlot, slotPosition.slotIndex);


            if(TooltipSystem.IsSelected(slotPosition))
            {
                TooltipSystem.Show(slotPosition, itemSlot,true);
            }
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
    public void UpdateWetness()
    {
        foreach (var container in containers)
        {
            var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.Value.entity);

            foreach (var item in buffer)
            {
                if (item.slot >= 0 && container.Value.itemSlots[item.slot] != null)
                {
                    var itemS = container.Value.itemSlots[item.slot];
                    itemS.wetness = item.wetness;
                    UIManager.instance.UpdateItemSlot(container.Value, itemS,item.slot);
                }

                if (EQHelperClient.ConvetSlotIndexToSelectedSlotIndex(item.slot) == selectedSlot.slotIndex && container.Value.gridIndex == selectedSlot.containerIndex)
                {
                    selectedItem.wetness = item.wetness;
                    DragManager.instance.UpdateSelected(selectedItem);
                }
            }

            
        }
        if (TooltipSystem.IsSlotPostion(out SlotPosition? slotPosition))
            TooltipSystem.Show(slotPosition.Value, GetItemStats(slotPosition.Value),true);
    }
    public void LocalUpdateSlotIndex(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container))
        {
            UIManager.instance.UpdateItemSlot(container, GetItemStats(slotPosition), slotPosition.slotIndex);
        }
    }
    private void SendMoveItem(SlotPosition from,SlotPosition to, int quantity)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        RPCHelper.SendRpc(ref entityCommandBuffer, new EQMoveItem() {
             to = to,
             from = from, 
             value = quantity
             });
        entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }
    private void SendMoveItem(SlotPosition to, int quantity)
    {
        SendMoveItem(SlotPosition.NullSlot,to,quantity);
    }
    public TooltipInfo GetTooltipInfo(int containerIndex)
    {
        if(containers.TryGetValue(containerIndex,out Container c))
            return c.GetTooltip();
        return null;
    }
    public void NewEvents(EqiupmentEventClient[] events, ref EntityCommandBuffer ecb)
    {
        foreach(var eqEevent in events)
        {
            switch (eqEevent.flag)
            {
                case 1:
                    if (eqEevent.slot >= 0)                          
                        NewEquipmentManager.instance.UpdateSlotIndex(new SlotPosition(eqEevent.containerIndex, eqEevent.slot));
                    break;
                case 2:
                        NewEquipmentManager.instance.ClearContainer(eqEevent.containerIndex,ref ecb);
                    break;
                case 3:
                        NewEquipmentManager.instance.ClearAllContainers(ref ecb);
                    break;
                case 4:
                        NewEquipmentManager.instance.UpdateWetness();
                    break;
            }
        }
    }

    #endregion    
}