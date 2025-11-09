using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public struct EQAddItem
{
    public SlotPosition pos;
    public int quantity;
    public bool slotExist;
    public EQAddItem(SlotPosition pos, int quantity, bool slotExist)
    {
        this.pos = pos;
        this.quantity = quantity;
        this.slotExist = slotExist;
    }

    public override string ToString()
    {
        return pos.ToString() + " " + quantity + " " + slotExist;
    }
}


public static class EQHelper
{
    public static int ConvetSlotIndexToSelectedSlotIndex(int slotIndex)
    {
        return -(slotIndex + 1);
    }

    public static PlayerContainers? GetPlayerContainer(BufferLookup<PlayerContainers> containersLookup, Entity player, int containerIndex)
    {
        if (!containersLookup.TryGetBuffer(player, out var containers))
            return null;

        PlayerContainers? container = null;
        foreach (var item in containers)
        {
            if (item.index == containerIndex)
            {
                container = item;
                break;
            }
        }
        return container;
    }

    public static List<int> GetPlayerContainers(ref SystemState state,BufferLookup<PlayerContainers> lookup, Entity player, int itemID)
    {
        List<int> result = new List<int>();
        var containers = lookup[player];
        foreach (var item in containers)
        {
            if (CheckRequirements(state.EntityManager.GetComponentData<ContainerComponent>(item.entity), itemID));
                result.Add(item.index);   
        }
        return result;
    }


    public static void PrintBuffer(BufferLookup<InventorySlot> slotsLookup, Entity container)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            Debug.Log(j + ". " + slot.ToString());
        }

    }


    public static bool TryGetBufferIndex(BufferLookup<InventorySlot> slotsLookup, BufferLookup<PlayerContainers> containers, Entity player, SlotPosition slotPosition, out InventorySlot? inventorySlot, out int bufferIndex)
    {
        var container = GetPlayerContainer(containers, player, slotPosition.containerIndex);
        if(container.HasValue)
            return TryGetBufferIndex(slotsLookup, slotPosition.slotIndex, container.Value.entity, out inventorySlot, out bufferIndex);
        inventorySlot = null;
        bufferIndex = -1;
        return false;
    }
    public static bool TryGetBufferIndex(BufferLookup<InventorySlot> slotsLookup, int slotIndex, Entity container, out int itemID, out int bufferIndex)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            if (slot.slot == slotIndex)
            {
                itemID = slot.itemId;
                bufferIndex = j;
                return true;
            }
        }
        itemID = -1;
        bufferIndex = -1;
        return false;
    }
    public static bool TryGetBufferIndex<T>(BufferLookup<T> slotsLookup, int slotIndex, Entity container, out T? bufforElement , out int bufferIndex) where T : unmanaged,IBufferElementData,IGetSlot
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            if (slot.GetSlot() == slotIndex)
            {
                bufforElement = slot;
                bufferIndex = j;
                return true;
            }
        }

        bufferIndex = -1;
        bufforElement = null;
        return false;
    }





    public static bool BufferContains(BufferLookup<InventorySlot> lookup,Entity container, int slotIndex) 
    {
        var slots = lookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            if (slot.slot == slotIndex) return true;
        }
        return false;
    }
    public static bool BufferContains(BufferLookup<InventorySlot> lookup, BufferLookup<PlayerContainers> containers, Entity player, SlotPosition pos)
    {
        var contaner = GetPlayerContainer(containers, player, pos.containerIndex);
        if (contaner.HasValue)
            return BufferContains(lookup, contaner.Value.entity, pos.slotIndex);
        return false;
    }

    public static EquipmentEvent[] ClearContainer(BufferLookup<InventorySlot> lookup, BufferLookup<PlayerContainers> containers, Entity player, int containerIndex)
    {
        var container = GetPlayerContainer(containers, player, containerIndex);
        if (container.HasValue)
        {
            lookup[container.Value.entity].Clear();
            return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(0, 2), containerIndex) };
        }
        return null;
    }
    public static EquipmentEvent[] ClearAllContainer(BufferLookup<InventorySlot> lookup, BufferLookup<PlayerContainers> containers, Entity player)
    {
        var playerContainers = containers[player];
        foreach (var container in playerContainers)
        {
            Debug.Log(container.index);
            lookup[container.entity].Clear();
        }
        return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(0, 3), 0) };
    }




    public static List<InventorySlot> TryGetAllItemsInContainer(BufferLookup<InventorySlot> slotsLookup, BufferLookup<PlayerContainers> containers, Entity player, int containerIndex, int itemID)
    {
        var container = GetPlayerContainer(containers, player, containerIndex);
        if(container.HasValue)
        {
            List<InventorySlot> items = new List<InventorySlot>();
            var slots = slotsLookup[container.Value.entity];
            for (int j = 0;j < slots.Length;j++)
            {
                var slot = slots.ElementAt(j);  
                if(slot.itemId == itemID)
                {
                    items.Add(slot);
                }
            }
            return items.OrderBy(x => x.slot).ToList();
        }
        return null;
    }




    public static EquipmentEvent[] MoveBetweenContainers(ref SystemState state,ref EntityCommandBuffer ecb, BufferLookup<InventorySlot> slotsLookup, Entity connection, PlayerContainers containersFrom, PlayerContainers containersTo,
    int slotTo, int slotFrom, int value,bool eventForSlotFrom = false)
    { 
        return MoveBetweenContainers(ref state,ref ecb,slotsLookup,connection,containersFrom,containersTo,slotTo,slotFrom,value,out int c,eventForSlotFrom);
    }
    public static EquipmentEvent[] MoveBetweenContainers(ref SystemState state,ref EntityCommandBuffer ecb, BufferLookup<InventorySlot> slotsLookup, Entity connection, PlayerContainers containersFrom, PlayerContainers containersTo,
    int slotTo, int slotFrom, int value, out int transferValue, bool eventForSlotFrom = false)
    {
        TryGetBufferIndex(slotsLookup, slotFrom, containersFrom.entity, out InventorySlot? itemFrom, out int fromIndex);
        TryGetBufferIndex(slotsLookup, slotTo, containersTo.entity, out InventorySlot? itemTo, out int toIndex);
        transferValue = 0;
        if (!itemFrom.HasValue) return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index) };
        

        Debug.Log(toIndex + " " + fromIndex);


        int number;
        int stackMax = ItemsAsset.instance.GetStackMax(itemFrom.Value.itemId);

        var containerCompoennent = state.EntityManager.GetComponentData<ContainerComponent>(containersTo.entity);
        if (CheckRequirements(containerCompoennent, itemFrom.Value.itemId))
        {
            if (fromIndex >= 0)
            {
                var fromBuffer = slotsLookup[containersFrom.entity];
                var toBuffer = slotsLookup[containersTo.entity];

                if (itemTo.HasValue && (itemFrom.Value.itemId != itemTo.Value.itemId || itemTo.Value.quantity >= stackMax))
                {
                    ref var slot = ref toBuffer.ElementAt(toIndex);
                    slot.slot = ConvetSlotIndexToSelectedSlotIndex(slotTo);
                    var entity = ecb.CreateEntity();
                    ecb.AddComponent(entity, new ReceiveRpcCommandRequest() { SourceConnection = connection });
                    ecb.AddComponent(entity, new EQSelectItem
                    {
                        position = new SlotPosition(containersTo.index, slot.slot),
                        value = slot.quantity
                    });
                    toIndex = -1;
                    value = itemFrom.Value.quantity;
                }
                else
                {
                    Debug.Log("Nie ma wymiany! " + slotFrom + "  " + slotTo + "  " + itemTo.HasValue + " " +(itemFrom.HasValue ? itemFrom.Value.itemId : " -1 " )+ " " + (itemTo.HasValue ? itemTo.Value.itemId :  " -1 ") );
                }


                ref InventorySlot from = ref fromBuffer.ElementAt(fromIndex);
                int to;
                value = Math.Clamp(value, 0, stackMax - (toIndex >= 0 ? toBuffer.ElementAt(toIndex).quantity : 0));

                transferValue = value;
                number = from.quantity - value;
                if (number > 0)
                {
                    from.quantity = number;
                    to = value;
                }
                else
                    to = from.quantity;


                Debug.Log(toIndex + " " + fromIndex);
                if (toIndex >= 0)
                {
                    toBuffer.ElementAt(toIndex).quantity += to;
                }
                else
                {
                    Debug.Log("new element!!!" + slotTo);
                    toBuffer.Add(new InventorySlot()
                    {
                        slot = slotTo,
                        itemId = itemFrom.Value.itemId,
                        quantity = to,
                    });
                }



                if (number <= 0)
                {
                    fromBuffer.RemoveAtSwapBack(fromIndex);
                }
            }

        }
        if (eventForSlotFrom)
        {
            return new EquipmentEvent[] {
                new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index),
                new EquipmentEvent(new EquipmentEventData(slotFrom, 1), containersFrom.index) };
        }
        else
        {
            return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index) };  
        }
    }
    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer,int networkID, params EquipmentEvent[] events)
    {
        if (events != null)
        {
            foreach (EquipmentEvent eventData in events)
            {
                eventData.SetNetworkID(networkID);
                Debug.Log("Event! Container" + eventData.containerIndex +  ", slot"+ eventData.data.slot);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData);
            }
        }
    }
    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer, int networkID, int duplicatedSlot, params EquipmentEvent[] events)
    {
        if (events != null)
        {
            bool first = true;
            foreach (EquipmentEvent eventData in events)
            {
                if(duplicatedSlot == eventData.data.slot)
                {
                    if (first) first = false;
                    else continue;
                }

                eventData.SetNetworkID(networkID);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData); 
            }
        }
    }
    public static EquipmentEvent[] AddItems(BufferLookup<ItemBarData> barsLookup,BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, EQAddItem[] values, int itemID)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        bool hasBar = ItemsAsset.instance.TryGetBarValues(itemID, out float startValue, out float maxValue);


        foreach (EQAddItem item in values)
        {
            Debug.Log(item.pos.ToString());
            if (item.slotExist)
            {
                var container = containers[player][item.pos.containerIndex];
                if (TryGetBufferIndex(slotLookup, item.pos.slotIndex, container.entity, out int id, out int bufferIndex))
                {
                    slotLookup[container.entity].ElementAt(bufferIndex).quantity += item.quantity;
                    equipmentEvents.Add(new EquipmentEvent(new EquipmentEventData(item.pos.slotIndex, 1), container.index));
                }
            }
        }

        foreach (EQAddItem item in values)
        {
            if (!item.slotExist)
            {
                var container = containers[player][item.pos.containerIndex];
                slotLookup[container.entity].Add(new InventorySlot()
                {
                    itemId = itemID,
                    quantity = item.quantity,
                    slot = item.pos.slotIndex,
                });
                if(hasBar)
                {
                    barsLookup[container.entity].Add(new ItemBarData() {
                        slot = item.pos.slotIndex,
                        value = startValue,
                        maxValue = maxValue
                    });
                }

                equipmentEvents.Add(new EquipmentEvent(new EquipmentEventData(item.pos.slotIndex, 1), container.index));
            }
        }

        return equipmentEvents.ToArray();
    }
    public static EquipmentEvent[] MoveItems(ref SystemState state,ref EntityCommandBuffer ecb, BufferLookup<InventorySlot> slotLookup,Entity connection, BufferLookup<PlayerContainers> containers,SlotPosition from, Entity player, EQAddItem[] values, int itemID)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        var containerFrom = GetPlayerContainer(containers, player, from.containerIndex);
        if(!containerFrom.HasValue) return null;

        foreach (EQAddItem item in values)
        {
            var containerTo = GetPlayerContainer(containers, player, item.pos.containerIndex);
            equipmentEvents.AddRange(MoveBetweenContainers(ref state,ref ecb, slotLookup, connection, containerFrom.Value,containerTo.Value,item.pos.slotIndex,from.slotIndex,item.quantity,true));
        }

        return equipmentEvents.ToArray();
    }
    public static EQAddItem[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, int itemID, int quantity)
    {
        var playerContainers = containers[player];
        int stackMax = ItemsAsset.instance.GetStackMax(itemID);
        int numberSlots = (int)Math.Ceiling((float)quantity / (float)stackMax);
        List<SlotPosition> freeSlots = new List<SlotPosition>();
        List<EQAddItem> moves = new List<EQAddItem>();

        for (int i = 0; i < playerContainers.Length; i++)
        {
            var container = playerContainers[i];
            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.entity);
            if (CheckRequirements(containerComponent, itemID))
            {
                var slots = slotLookup[container.entity];
                bool[] occupiedSlots = new bool[containerComponent.capacity];

                Dictionary<int, int> slotIndexToBuffer = new Dictionary<int, int>();
                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    occupiedSlots[slot.slot] = true;
                    if (slot.itemId == itemID && slot.quantity < stackMax)
                    {
                        slotIndexToBuffer.Add(slot.slot, j);
                    }
                }
                foreach (var slotToBuffor in slotIndexToBuffer.OrderBy(x => x.Key))
                {
                    var slot = slots[slotToBuffor.Value];
                    int free = stackMax - slot.quantity;
                    if (free >= quantity)
                    {
                        moves.Add(new EQAddItem(new SlotPosition(container.index, slot.slot), quantity, true));
                        return moves.ToArray();
                    }
                    else
                    {
                        moves.Add(new EQAddItem(new SlotPosition(container.index, slot.slot), free, true));
                        quantity -= free;
                    }
                }

                if (numberSlots > 0)
                {
                    for (int j = 0; j < occupiedSlots.Length; j++)
                    {
                        if (!occupiedSlots[j])
                        {
                            freeSlots.Add(new SlotPosition(container.index, j));
                            numberSlots--;
                            if (numberSlots == 0) break;
                        }
                    }
                }
            }
        }

        foreach (var item in freeSlots)
        {
            if(quantity > stackMax)
            {
                moves.Add(new EQAddItem(item, stackMax, false));
                quantity -= stackMax;
            }
            else
            {
                moves.Add(new EQAddItem(item, quantity,false));
                break;
            }
        }
        return moves.ToArray(); 
    }
    public static EQAddItem[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, int itemID, int quantity, params int[] findIncontainers)
    {
        var playerContainers = containers[player];
        int stackMax = ItemsAsset.instance.GetStackMax(itemID);
        int numberSlots = (int)Math.Ceiling((float)quantity / (float)stackMax);
        List<SlotPosition> freeSlots = new List<SlotPosition>();
        List<EQAddItem> moves = new List<EQAddItem>();

        for (int i = 0; i < findIncontainers.Length; i++)
        {
            PlayerContainers? container = null;
            for (int j = 0; j < playerContainers.Length; j++)
            {
                if (playerContainers[j].index == findIncontainers[i])
                    container = playerContainers[j];
            }
            if (!container.HasValue) continue;

            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.Value.entity);
            if (CheckRequirements(containerComponent, itemID))
            {
                var slots = slotLookup[container.Value.entity];
                bool[] occupiedSlots = new bool[containerComponent.capacity];
                
                Dictionary<int,int> slotIndexToBuffer = new Dictionary<int, int>();
                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    occupiedSlots[slot.slot] = true;
                    if (slot.itemId == itemID && slot.quantity < stackMax)
                    {
                        slotIndexToBuffer.Add(slot.slot,j);
                    }
                }
                foreach (var slotToBuffor in slotIndexToBuffer.OrderBy(x => x.Key))
                {
                    var slot = slots[slotToBuffor.Value];
                    int free = stackMax - slot.quantity;
                    if (free >= quantity)
                    {
                        moves.Add(new EQAddItem(new SlotPosition(container.Value.index, slot.slot), quantity, true));
                        return moves.ToArray();
                    }
                    else
                    {
                        moves.Add(new EQAddItem(new SlotPosition(container.Value.index, slot.slot), free, true));
                        quantity -= free;
                    }    
                }

                if (numberSlots > 0)
                {
                    for (int j = 0; j < occupiedSlots.Length; j++)
                    {
                        if (!occupiedSlots[j])
                        {
                            freeSlots.Add(new SlotPosition(container.Value.index, j));
                            numberSlots--;
                            if (numberSlots == 0) break;
                        }
                    }
                }
            }
        }

        foreach (var item in freeSlots)
        {
            if (quantity > stackMax)
            {
                moves.Add(new EQAddItem(item, stackMax, false));
                quantity -= stackMax;
            }
            else
            {
                moves.Add(new EQAddItem(item, quantity, false));
                break;
            }
        }
        return moves.ToArray();
    }
    public static bool CheckRequirements(ContainerComponent containerComponent, int itemID)
    {
        return CheckRequirements(containerComponent.mandatoryProperties, containerComponent.mandatoryData, itemID);
    }
    public static bool CheckRequirements(MandatoryProperties mandatoryProperties, int mandatoryData, int itemID)
    {
        switch (mandatoryProperties)
        {
            case MandatoryProperties.none:
                return true;
            case MandatoryProperties.tag:
                return ItemsAsset.instance.ItemHasTheTag(itemID, mandatoryData);
            case MandatoryProperties.item:
                return itemID == mandatoryData;
        }
        return false;
    }
    public static void Deselection(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, BufferLookup<PlayerContainers> containers, Entity player,int networkID, Entity connection)
    {
        var selectedSlot = state.EntityManager.GetComponentData<ContainerSettings>(player);

        if (!selectedSlot.Position.Compare(SlotPosition.NullSlot))
        {
            int slotIndex = ConvetSlotIndexToSelectedSlotIndex(selectedSlot.Position.slotIndex);

            if (TryGetBufferIndex(slotsLookup, containers, player, selectedSlot.Position, out InventorySlot? slot, out int index))
            {
                TryGetBufferIndex(slotsLookup, containers, player, new SlotPosition(selectedSlot.Position.containerIndex, slotIndex), out InventorySlot? outSlot, out int bufferIndex);
                int quantity = slot.Value.quantity;

                if (!outSlot.HasValue || outSlot.Value.itemId == slot.Value.itemId)
                {
                    var container = GetPlayerContainer(containers, player, selectedSlot.Position.containerIndex);
                    if (container.HasValue)
                    {
                        var events = MoveBetweenContainers(ref state, ref entityCommandBuffer, slotsLookup, connection, container.Value, container.Value, slotIndex, selectedSlot.Position.slotIndex, quantity, out int transferValue);
                        quantity -= transferValue;
                        SendEvents(ref entityCommandBuffer, networkID,events);
                    }
                }
                if (quantity > 0)
                {
                    var items = FindSlotForItem(ref state, slotsLookup, containers, player, slot.Value.itemId, quantity);
                    var events = MoveItems(ref state, ref entityCommandBuffer, slotsLookup, connection, containers, selectedSlot.Position, player, items, slot.Value.itemId);
                    SendEvents(ref entityCommandBuffer, networkID, events);
                }
            }
        }
    }
}