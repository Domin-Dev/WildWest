using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;

public struct EQTransferData
{
    public SlotPosition pos;
    public int quantity;
    public bool slotExist;

    public EQTransferData(SlotPosition pos, int quantity, bool slotExist)
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
   
    public static bool TryGetPlayerContainer(BufferLookup<PlayerContainers> containersLookup, Entity player,int containerIndex, out PlayerContainers? playerContainer)
    {
        playerContainer  = GetPlayerContainer(containersLookup,player,containerIndex);
        return playerContainer.HasValue;
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
   
    public static bool TryFindContainerForItem(ref SystemState state,BufferLookup<PlayerContainers> lookup, Entity player,out PlayerContainers? playerContainer, ContainerType containerType, int itemID)
    {
        var containers = lookup[player];
        foreach (var item in containers)
        {
            var containerComp = state.EntityManager.GetComponentData<ContainerComponent>(item.entity);
            if(containerComp.containerType == containerType && CheckRequirements(containerComp, itemID))
            {
                playerContainer = item;
                return true;
            }
        }
        playerContainer = null;
        return false;
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
    public static bool TryGetBufferIndex<T>(BufferLookup<T> lookup, int slotIndex, Entity container, out T? bufforElement , out int bufferIndex) where T : unmanaged,IBufferElementData,IGetSlot
    {
        var slots = lookup[container];
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

    public static EquipmentEvent[] ClearContainer(BufferLookup<ItemBarData> bars,BufferLookup<InventorySlot> lookup, BufferLookup<PlayerContainers> containers, Entity player, int containerIndex)
    {
        var container = GetPlayerContainer(containers, player, containerIndex);
        if (container.HasValue)
        {
            lookup[container.Value.entity].Clear();
            bars[container.Value.entity].Clear();

            return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(0, 2), containerIndex) };
        }
        return null;
    }
    public static EquipmentEvent[] ClearAllContainer(BufferLookup<ItemBarData> bars,BufferLookup<InventorySlot> lookup, BufferLookup<PlayerContainers> containers, Entity player)
    {
        var playerContainers = containers[player];
        foreach (var container in playerContainers)
        {
            Debug.Log(container.index);
            lookup[container.entity].Clear();
            bars[container.entity].Clear();
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


    public static EquipmentEvent[] MoveBetweenContainers(ref SystemState state,ref EntityCommandBuffer ecb, BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, Entity connection, PlayerContainers containersFrom, PlayerContainers containersTo,
    int slotTo, int slotFrom, int value = int.MaxValue,bool eventForSlotFrom = false)
    { 
        return MoveBetweenContainers(ref state,ref ecb,barsLookup,slotsLookup,connection,containersFrom,containersTo,slotTo,slotFrom,value,out int c,eventForSlotFrom);
    }
    public static EquipmentEvent[] MoveBetweenContainers(ref SystemState state,ref EntityCommandBuffer ecb, BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, Entity connection, PlayerContainers containersFrom, PlayerContainers containersTo,
    int slotTo, int slotFrom, int value, out int transferValue, bool eventForSlotFrom = false)
    {

        Debug.Log("<Color=red> move  " + slotFrom + "    "+  slotTo );
        TryGetBufferIndex(slotsLookup, slotFrom, containersFrom.entity, out InventorySlot? itemFrom, out int fromIndex);
        TryGetBufferIndex(slotsLookup, slotTo, containersTo.entity, out InventorySlot? itemTo, out int toIndex);
        bool haveBarFrom =  TryGetBufferIndex(barsLookup, slotFrom, containersFrom.entity, out var barFrom, out int barFromIndex);
        bool haveBarTo = TryGetBufferIndex(barsLookup, slotTo, containersTo.entity, out var barTo, out int barToIndex);

        transferValue = 0;
        if (!itemFrom.HasValue) return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index) };
        
        int number;
        int stackMax = ItemsAsset.instance.GetStackMax(itemFrom.Value.itemId);
        var containerCompoennent = state.EntityManager.GetComponentData<ContainerComponent>(containersTo.entity);
        int startToIndex = toIndex; 


        if (CheckRequirements(containerCompoennent, itemFrom.Value.itemId))
        {
            if (fromIndex >= 0)
            {
                var fromBuffer = slotsLookup[containersFrom.entity];
                var toBuffer = slotsLookup[containersTo.entity];

                // swap selected item
                if (itemTo.HasValue && (itemFrom.Value.itemId != itemTo.Value.itemId || itemFrom.Value.quality != itemTo.Value.quality || itemTo.Value.quantity >= stackMax))
                {
                    // if is selected by player
                    ref var slot = ref toBuffer.ElementAt(toIndex);
                    if(slotFrom < 0)
                    {
                        slot.slot = EQHelperClient.ConvetSlotIndexToSelectedSlotIndex(slotTo);
                        if(haveBarTo)
                            barsLookup[containersTo.entity].ElementAt(barToIndex).slot = slot.slot;

                        var entity = ecb.CreateEntity();
                        ecb.AddComponent(entity, new ReceiveRpcCommandRequest() { SourceConnection = connection });
                        ecb.AddComponent(entity, new EQSelectItem
                        {
                            position = new SlotPosition(containersTo.index, slot.slot),
                            value = slot.quantity
                        });
                    }
                    // swap slots 
                    else
                    {
                        if (containersFrom.index == containersTo.index)
                        {
                            slot.slot = slotFrom;
                            if(haveBarTo)
                                barsLookup[containersTo.entity].ElementAt(barToIndex).slot = slot.slot;
                        }
                        else
                        {
                            fromBuffer.Add(new InventorySlot()
                            {
                                slot = slotFrom,
                                itemId = itemTo.Value.itemId,
                                quantity = itemTo.Value.quantity,
                                quality = itemTo.Value.quality,
                                wetness = itemTo.Value.wetness
                            });

                            if (haveBarTo)
                            {
                                barsLookup[containersFrom.entity].Add(new ItemBarData()
                                {
                                    slot = slotFrom,
                                    maxValue = barTo.Value.maxValue,
                                    value = barTo.Value.value
                                });
                            }
                        }
                    }

                    toIndex = -1;
                    value = itemFrom.Value.quantity;
                }


                ref InventorySlot from = ref fromBuffer.ElementAt(fromIndex);
                int to;
                value = Math.Clamp(value, 0, stackMax - (toIndex >= 0 ? toBuffer.ElementAt(toIndex).quantity : 0));


                // transfer value calculations
                transferValue = value;
                number = from.quantity - value;
                if (number > 0)
                {
                    from.quantity = number;
                    to = value;
                }
                else
                    to = from.quantity;
            
                // slot "to" contains something
                if (toIndex >= 0)
                {
                    ref var i = ref toBuffer.ElementAt(toIndex);
                    if (haveBarTo)
                        barsLookup[containersTo.entity].ElementAt(barToIndex).value = EQHelperClient.CalculateMixPercentage(i.quantity, barTo.Value.value, to, barFrom.Value.value);
                    
                    i.wetness = EQHelperClient.CalculateMixPercentage(i.quantity, i.wetness, to, from.wetness);
                    i.quantity += to;

                    if (number <= 0)
                    {
                        fromBuffer.RemoveAtSwapBack(fromIndex);
                        if (haveBarFrom) barsLookup[containersFrom.entity].RemoveAtSwapBack(barFromIndex);
                    }
                }
                // slot "to" is empty
                else
                {
                    if (containersFrom.index == containersTo.index && number <= 0)
                    {
                        from.slot = slotTo;
                        if(haveBarFrom) barsLookup[containersFrom.entity].ElementAt(barFromIndex).slot = slotTo;
                    }
                    else
                    { 
                        toBuffer.Add(new InventorySlot()
                        {
                            slot = slotTo,
                            itemId = itemFrom.Value.itemId,
                            quantity = to,
                            quality = itemFrom.Value.quality,
                            wetness = itemFrom.Value.wetness
                        });

                        if (haveBarFrom)
                        {
                            barsLookup[containersTo.entity].Add(new ItemBarData()
                            {
                                slot = slotTo,
                                maxValue = barFrom.Value.maxValue,
                                value = barFrom.Value.value
                            });
                        }

                        if (number <= 0)
                        {
                            fromBuffer.RemoveAtSwapBack(fromIndex);
                            if (haveBarFrom) barsLookup[containersFrom.entity].RemoveAtSwapBack(barFromIndex);

                            if(slotFrom >= 0 && startToIndex >= 0)
                            {
                                toBuffer.RemoveAtSwapBack(startToIndex);
                                if (haveBarTo) barsLookup[containersTo.entity].RemoveAtSwapBack(barToIndex);
                            }
                        }
                    }
                }

                // create events
                NewItemInTheSlot(ref ecb, containerCompoennent,slotTo,connection);
            }

        }
        if (eventForSlotFrom)
        {
                    Debug.Log("<Color=red> move  " + slotFrom + "    "+  slotTo );
                    Debug.Log("<Color=red> move  " + containersFrom.index + "    "+  containersTo.index );
            return new EquipmentEvent[] {
                
                new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index),
                new EquipmentEvent(new EquipmentEventData(slotFrom, 1), containersFrom.index) };
        }
        else
        {
            return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index) };  
        }
    }



    public static void NewItemInTheSlot(ref EntityCommandBuffer ecb,ContainerComponent containerComponent, int slotPos, Entity connection)
    {
        if(containerComponent.containerType == ContainerType.Outfit)
            EntityHelper.CreateEntityWithComponent(ref ecb, new EQOnEquip()
            {
                slotPosition = new SlotPosition(containerComponent.containerIndex,slotPos),
                connection = connection
            });
    }

    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer,int networkID, params EquipmentEvent[] events)
    {
        {
            foreach (EquipmentEvent eventData in events)
            {
                eventData.SetNetworkID(networkID);
                Debug.Log("Event! Container" + eventData.containerIndex +  ", slot"+ eventData.data.slot);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData);
            }
        }
    }
    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer, int networkID, SlotPosition duplicatedPos, params EquipmentEvent[] events)
    {
        if (events != null)
        {
            bool first = true;
            foreach (EquipmentEvent eventData in events)
            {
                if(duplicatedPos.Compare(eventData.slotPosition))
                {
                    if (first) first = false;
                    else continue;
                }
                eventData.SetNetworkID(networkID);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData); 
            }
        }
    }
    
    public static void Rain(ref SystemState state,float intensity, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containersLookup, Entity player)
    {
        var containers = containersLookup[player];
        foreach (var container in containers)
        {
            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.entity);
            var slots = slotLookup[container.entity];
            float value = intensity * (1f - containerComponent.waterResistance * 0.01f);

            if (Math.Abs(value) > 0.0001f)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    slots.ElementAt(i).UpdateWetness(value);
                }
            }
        }
    }
    public static EquipmentEvent[] AddItems(BufferLookup<ItemBarData> barsLookup,BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, EQTransferData[] values, EQGiveItem itemData)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        bool hasBar = ItemsAsset.instance.TryGetBarValues(itemData.item.itemId, out float startValue, out float maxValue);
        float barVal = maxValue * (Math.Clamp(itemData.barValue, 0f, 1f));


        foreach (EQTransferData item in values)
        {
            if (item.slotExist && TryGetPlayerContainer(containers,player,item.pos.containerIndex,out PlayerContainers? container))
            {
                if (TryGetBufferIndex(slotLookup, item.pos.slotIndex, container.Value.entity, out int id, out int bufferIndex))
                {
                    ref var i = ref slotLookup[container.Value.entity].ElementAt(bufferIndex);

                    if (hasBar && TryGetBufferIndex(barsLookup, item.pos.slotIndex, container.Value.entity, out var barValue, out int barIndex))
                    {
                        barsLookup[container.Value.entity].ElementAt(barIndex).value = EQHelperClient.CalculateMixPercentage(i.quantity,barValue.Value.value,item.quantity, barVal);
                    }

                    i.wetness = EQHelperClient.CalculateMixPercentage(i.quantity, i.wetness, item.quantity, itemData.item.wetness);             
                    i.quantity += item.quantity;
                    equipmentEvents.Add(new EquipmentEvent(new EquipmentEventData(item.pos.slotIndex, 1), container.Value.index));
                }
            }
        }

        foreach (EQTransferData item in values)
        {
            if (!item.slotExist &&  TryGetPlayerContainer(containers,player,item.pos.containerIndex,out PlayerContainers? container))
            {             
                slotLookup[container.Value.entity].Add(new InventorySlot()
                {
                    itemId = itemData.item.itemId,
                    quantity = item.quantity,
                    slot = item.pos.slotIndex,
                    wetness = itemData.item.wetness,
                    quality = itemData.item.quality,
                });
                if(hasBar)
                {
                    barsLookup[container.Value.entity].Add(new ItemBarData() {
                        slot = item.pos.slotIndex,
                        value = barVal,
                        maxValue = maxValue
                    });
                }
                equipmentEvents.Add(new EquipmentEvent(new EquipmentEventData(item.pos.slotIndex, 1), container.Value.index));
            }
        }

        return equipmentEvents.ToArray();
    }
    public static EquipmentEvent[] MoveItems(ref SystemState state,ref EntityCommandBuffer ecb,BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotLookup,Entity connection, BufferLookup<PlayerContainers> containers,SlotPosition from, Entity player, EQTransferData[] values)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        var containerFrom = GetPlayerContainer(containers, player, from.containerIndex);
        if(!containerFrom.HasValue) return null;

        foreach (EQTransferData item in values)
        {
            var containerTo = GetPlayerContainer(containers, player, item.pos.containerIndex);
            equipmentEvents.AddRange(MoveBetweenContainers(ref state,ref ecb, barsLookup,slotLookup, connection, containerFrom.Value,containerTo.Value,item.pos.slotIndex,from.slotIndex,item.quantity,true));
        }

        return equipmentEvents.ToArray();
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player,int itemID,int quantity = 1, byte wetness = 0, Quality quality = Quality.none)
    {
        return FindSlotForItem(ref state, slotLookup, containers, player, new InventorySlot() {
            itemId = itemID,
            quantity = quantity,
            wetness = wetness,
            quality = quality
        });
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, int itemID, int quantity = 1, byte wetness = 0, Quality quality = Quality.none, params int[] findIncontainers)
    {
        return FindSlotForItem(ref state, slotLookup, containers, player, new InventorySlot()
        {
            itemId = itemID,
            quantity = quantity,
            wetness = wetness,
            quality = quality
        },findIncontainers);
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, InventorySlot inventorySlot)
    {
        var playerContainers = containers[player];
        int stackMax = ItemsAsset.instance.GetStackMax(inventorySlot.itemId);
        int numberSlots = (int)Math.Ceiling((float)inventorySlot.quantity / (float)stackMax);
        List<SlotPosition> freeSlots = new List<SlotPosition>();
        List<EQTransferData> moves = new List<EQTransferData>();

        for (int i = 0; i < playerContainers.Length; i++)
        {
            var container = playerContainers[i];
            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.entity);
            if(containerComponent.containerType != ContainerType.Standard) continue;
            
            if (CheckRequirements(containerComponent, inventorySlot.itemId))
            {
                var slots = slotLookup[container.entity];
                bool[] occupiedSlots = new bool[containerComponent.capacity];

                Dictionary<int, int> slotIndexToBuffer = new Dictionary<int, int>();
                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    occupiedSlots[slot.slot] = true;
                    if (slot.itemId == inventorySlot.itemId && inventorySlot.quality == slot.quality && slot.quantity < stackMax)
                    {
                        slotIndexToBuffer.Add(slot.slot, j);
                    }
                }
                foreach (var slotToBuffor in slotIndexToBuffer.OrderBy(x => x.Key))
                {
                    var slot = slots[slotToBuffor.Value];
                    int free = stackMax - slot.quantity;
                    if (free >= inventorySlot.quantity)
                    {
                        moves.Add(new EQTransferData(new SlotPosition(containerComponent.containerIndex, slot.slot), inventorySlot.quantity, true));
                        return moves.ToArray();
                    }
                    else
                    {
                        moves.Add(new EQTransferData(new SlotPosition(containerComponent.containerIndex, slot.slot), free, true));
                        inventorySlot.quantity -= free;
                    }
                }

                if (numberSlots > 0)
                {
                    for (int j = 0; j < occupiedSlots.Length; j++)
                    {
                        if (!occupiedSlots[j])
                        {
                            freeSlots.Add(new SlotPosition(i, j));
                            numberSlots--;
                            if (numberSlots == 0) break;
                        }
                    }
                }
            }
        }

        foreach (var item in freeSlots)
        {
            if(inventorySlot.quantity > stackMax)
            {
                moves.Add(new EQTransferData(item, stackMax, false));
                inventorySlot.quantity -= stackMax;
            }
            else
            {
                moves.Add(new EQTransferData(item, inventorySlot.quantity, false));
                break;
            }
        }
        return moves.ToArray(); 
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, InventorySlot inventorySlot, params int[] findIncontainers)
    {
        var playerContainers = containers[player];
        int stackMax = ItemsAsset.instance.GetStackMax(inventorySlot.itemId);
        int numberSlots = (int)Math.Ceiling((float)inventorySlot.quantity / (float)stackMax);
        List<SlotPosition> freeSlots = new List<SlotPosition>();
        List<EQTransferData> moves = new List<EQTransferData>();

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
            if (CheckRequirements(containerComponent, inventorySlot.itemId))
            {
                var slots = slotLookup[container.Value.entity];
                bool[] occupiedSlots = new bool[containerComponent.capacity];
                
                Dictionary<int,int> slotIndexToBuffer = new Dictionary<int, int>();
                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    occupiedSlots[slot.slot] = true;
                    if (slot.itemId == inventorySlot.itemId && slot.quality == inventorySlot.quality && slot.quantity < stackMax)
                    {
                        slotIndexToBuffer.Add(slot.slot,j);
                    }
                }
                foreach (var slotToBuffor in slotIndexToBuffer.OrderBy(x => x.Key))
                {
                    var slot = slots[slotToBuffor.Value];
                    int free = stackMax - slot.quantity;
                    if (free >= inventorySlot.quantity)
                    {
                        moves.Add(new EQTransferData(new SlotPosition(container.Value.index, slot.slot), inventorySlot.quantity, true));
                        return moves.ToArray();
                    }
                    else
                    {
                        moves.Add(new EQTransferData(new SlotPosition(container.Value.index, slot.slot), free, true));
                        inventorySlot.quantity -= free;
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
            if (inventorySlot.quantity > stackMax)
            {
                moves.Add(new EQTransferData(item, stackMax, false));
                inventorySlot.quantity -= stackMax;
            }
            else
            {
                moves.Add(new EQTransferData(item, inventorySlot.quantity, false));
                break;
            }
        }
        return moves.ToArray();
    }
    public static bool TryFindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<PlayerContainers> containers, Entity player, InventorySlot inventorySlot,out EQTransferData[] data, params int[] findIncontainers)
    {
        data = FindSlotForItem(ref state,slotLookup ,containers,player,inventorySlot, findIncontainers);
        return data.Length > 0;
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
    public static void Deselection(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer,BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, BufferLookup<PlayerContainers> containers, Entity player,int networkID, Entity connection)
    {
        var selectedSlot = state.EntityManager.GetComponentData<ContainerSettings>(player);

        if (!selectedSlot.Position.Compare(SlotPosition.NullSlot))
        {
            int slotIndex = EQHelperClient.ConvetSlotIndexToSelectedSlotIndex(selectedSlot.Position.slotIndex);

            if (TryGetBufferIndex(slotsLookup, containers, player, selectedSlot.Position, out InventorySlot? slot, out int index))
            {
                TryGetBufferIndex(slotsLookup, containers, player, new SlotPosition(selectedSlot.Position.containerIndex, slotIndex), out InventorySlot? outSlot, out int bufferIndex);
                int quantity = slot.Value.quantity;

                if (!outSlot.HasValue || outSlot.Value.itemId == slot.Value.itemId)
                {
                    var container = GetPlayerContainer(containers, player, selectedSlot.Position.containerIndex);
                    if (container.HasValue)
                    {
                        var events = MoveBetweenContainers(ref state, ref entityCommandBuffer, barsLookup, slotsLookup, connection, container.Value, container.Value, slotIndex, selectedSlot.Position.slotIndex, quantity, out int transferValue);
                        quantity -= transferValue;
                        SendEvents(ref entityCommandBuffer, networkID,events);
                    }
                }
                if (quantity > 0)
                {
                    var items = FindSlotForItem(ref state, slotsLookup, containers, player, slot.Value.itemId, quantity);
                    var events = MoveItems(ref state, ref entityCommandBuffer,barsLookup, slotsLookup, connection, containers, selectedSlot.Position, player, items);
                    SendEvents(ref entityCommandBuffer, networkID, events);
                }
            }
        }
    }
}