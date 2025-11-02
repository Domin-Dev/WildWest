using System;
using System.Collections.Generic;
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
                itemID = slot.ItemId;
                bufferIndex = j;
                return true;
            }
        }
        itemID = -1;
        bufferIndex = -1;
        return false;
    }
    public static bool TryGetBufferIndex(BufferLookup<InventorySlot> slotsLookup, int slotIndex, Entity container, out InventorySlot? inventorySlot , out int bufferIndex)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            if (slot.slot == slotIndex)
            {
                inventorySlot = slot;
                bufferIndex = j;
                return true;
            }
        }
        bufferIndex = -1;
        inventorySlot = null;
        return false;
    }


    public static EquipmentEvent[] MoveBetweenContainers(ref EntityCommandBuffer ecb, BufferLookup<InventorySlot> slotsLookup, Entity connection, PlayerContainers containersFrom, PlayerContainers containersTo,
    int slotTo, int slotFrom, int value,bool eventForSlotFrom = false)
    { 
        return MoveBetweenContainers(ref ecb,slotsLookup,connection,containersFrom,containersTo,slotTo,slotFrom,value,out int c,eventForSlotFrom);
    }
    public static EquipmentEvent[] MoveBetweenContainers(ref EntityCommandBuffer ecb, BufferLookup<InventorySlot> slotsLookup, Entity connection, PlayerContainers containersFrom, PlayerContainers containersTo,
    int slotTo, int slotFrom, int value, out int transferValue, bool eventForSlotFrom = false)
    { 
        TryGetBufferIndex(slotsLookup, slotFrom, containersFrom.entity, out InventorySlot? itemFrom, out int fromIndex);
        TryGetBufferIndex(slotsLookup, slotTo, containersTo.entity, out InventorySlot? itemTo, out int toIndex);
        transferValue = 0;
        if (!itemFrom.HasValue) return null;

        int number;
        int stackMax = ItemsAsset.instance.GetStackMax(itemFrom.Value.ItemId);

        if (fromIndex >= 0)
        {
            var fromBuffer = slotsLookup[containersFrom.entity];
            var toBuffer = slotsLookup[containersTo.entity];
            if (itemTo.HasValue && (itemFrom.Value.ItemId != itemTo.Value.ItemId || itemTo.Value.quantity >= stackMax))
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
            }


            ref InventorySlot from = ref fromBuffer.ElementAt(fromIndex);
            int to;
            Debug.Log("value " + value);
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

            if (toIndex >= 0)
            {
                toBuffer.ElementAt(toIndex).quantity += to;
            }
            else
                toBuffer.Add(new InventorySlot()
                {
                    slot = slotTo,
                    ItemId = itemFrom.Value.ItemId,
                    quantity = to,
                });

            if (number <= 0)
                fromBuffer.RemoveAtSwapBack(fromIndex);
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
    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer,EquipmentEvent[] events,int networkID)
    {
        if (events != null)
        {
            foreach (EquipmentEvent eventData in events)
            {
                eventData.SetNetworkID(networkID);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData);
            }
        }
    }
    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer, EquipmentEvent[] events, int networkID, int duplicatedSlot)
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
    public static EquipmentEvent[] AddItems(BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, EQAddItem[] values, int itemID)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;

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
                    ItemId = itemID,
                    quantity = item.quantity,
                    slot = item.pos.slotIndex,
                });
                equipmentEvents.Add(new EquipmentEvent(new EquipmentEventData(item.pos.slotIndex, 1), container.index));
            }
        }
        return equipmentEvents.ToArray();
    }
    public static EquipmentEvent[] MoveItems(ref EntityCommandBuffer ecb, BufferLookup<InventorySlot> slotLookup,Entity connection, BufferLookup<PlayerContainers> containers,SlotPosition from, Entity player, EQAddItem[] values, int itemID)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        var containerFrom = GetPlayerContainer(containers, player, from.containerIndex);
        if(!containerFrom.HasValue) return null;

        foreach (EQAddItem item in values)
        {
            Debug.Log(item.pos.ToString());
            var containerTo = GetPlayerContainer(containers, player, item.pos.containerIndex);
            equipmentEvents.AddRange(MoveBetweenContainers(ref ecb, slotLookup, connection, containerFrom.Value,containerTo.Value,item.pos.slotIndex,from.slotIndex,item.quantity,true));
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
            if(CheckRequirements(containerComponent, itemID))
            {
                var slots = slotLookup[container.entity];
                bool[] occupiedSlots = new bool[containerComponent.capacity];  

                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    occupiedSlots[slot.slot] = true;
                    if(slot.ItemId == itemID && slot.quantity < stackMax)
                    {
                        int free = stackMax - slot.quantity;
                        if(free >= quantity)
                        {
                            moves.Add(new EQAddItem(new SlotPosition(container.index, slot.slot), quantity,true));
                            return moves.ToArray();
                        }
                        else
                        {
                            moves.Add(new EQAddItem(new SlotPosition(container.index, slot.slot), free,true));
                            quantity -= free;
                        }
                    }
                }
                if(numberSlots > 0)
                {
                    for (int j = 0;j < occupiedSlots.Length; j++)
                    {
                        if (!occupiedSlots[j])
                        {
                            freeSlots.Add(new SlotPosition(container.index, j));
                            numberSlots--;
                            if(numberSlots == 0) break; 
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

                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    occupiedSlots[slot.slot] = true;
                    if (slot.ItemId == itemID && slot.quantity < stackMax)
                    {
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
        switch (containerComponent.mandatoryProperties)
        {
            case MandatoryProperties.none:
                return true;
            case MandatoryProperties.tag:
                return ItemsAsset.instance.ItemHasTheTag(itemID, containerComponent.mandatoryData);
            case MandatoryProperties.item:
                return itemID == containerComponent.mandatoryData;
        }
        return false;
    }
}