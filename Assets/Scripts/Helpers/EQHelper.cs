using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

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

public struct SlotData
{
    public EQTransferData transferData;
    public int itemID;

    public SlotData(EQTransferData transferData, int itemID)
    {
        this.transferData = transferData;
        this.itemID = itemID;
    }
}


public static class EQHelper
{
    public static bool PlayerHasTheAmmo(int ammoID,InventorySlot[] slots)
    {
        return PlayerHasTheAmmo(ammoID,slots,out var index);
    }
    public static bool PlayerHasTheAmmo(int ammoID,InventorySlot[] slots, out int index)
    {
        for(int i = 0; i < slots.Length;i++)
        {
            if(slots[i].itemId == ammoID)
            {
                index = i;
                return true;   
            }
        }
        index = -1;
        return false;
    }
    public static bool PlayerHasTheAmmo(int ammoID,SlotData[] slots, out int index)
    {
        for(int i = 0; i < slots.Length;i++)
        {
            if(slots[i].itemID == ammoID)
            {
                index = i;
                return true;   
            }
        }
        index = -1;
        return false;
    }



    public static bool TryGetPlayerContainer(BufferLookup<EntityContainers> containersLookup, Entity player,int containerIndex, out EntityContainers? playerContainer)
    {
        playerContainer = GetContainer(containersLookup,player,containerIndex);
        return playerContainer.HasValue;
    }
    public static EntityContainers? GetContainer(BufferLookup<EntityContainers> containersLookup, Entity player, int containerIndex)
    {
        if (!containersLookup.TryGetBuffer(player, out var containers))
            return null;

        EntityContainers? container = null;
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
    public static EntityContainers? GetPlayerContainer(BufferLookup<EntityContainers> containersLookup, Entity player, SlotPosition containerIndex)
    {
        return GetContainer(containersLookup,player,containerIndex.containerIndex);
    }

    public static List<int> GetPlayerContainers(ref SystemState state,BufferLookup<EntityContainers> lookup, Entity player, int itemID)
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
   


    //fixed
    public static bool TryFindContainerForItem(ref SystemState state,BufferLookup<EntityContainers> lookup, Entity player,out EntityContainers? playerContainer, ContainerType containerType, int itemID)
    {
        var containers = lookup[player];
        foreach (var item in containers)
        {
            var containerComp = state.EntityManager.GetComponentData<ContainerComponent>(item.entity);
            if(!containerComp.serverContainer && containerComp.containerType == containerType && CheckRequirements(containerComp, itemID))
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
        }

    }
    public static bool TryGetBufferIndex(BufferLookup<InventorySlot> slotsLookup, BufferLookup<EntityContainers> containers, Entity player, SlotPosition slotPosition, out InventorySlot? inventorySlot, out int bufferIndex)
    {
        return TryGetBufferIndex(slotsLookup,containers,player,slotPosition,out inventorySlot, out bufferIndex,out var containerEntity);
    }
    public static bool TryGetBufferIndex(BufferLookup<InventorySlot> slotsLookup, BufferLookup<EntityContainers> containers, Entity player, SlotPosition slotPosition, out InventorySlot? inventorySlot, out int bufferIndex, out Entity containerEntity)
    {
        var container = GetContainer(containers, player, slotPosition.containerIndex);
        containerEntity = Entity.Null;
        if(container.HasValue)
        {
            containerEntity = container.Value.entity;
            return TryGetBufferIndex(slotsLookup, slotPosition.slotIndex, container.Value.entity, out inventorySlot, out bufferIndex);
        }
        inventorySlot = null;
        bufferIndex = -1;
        return false;
    }
    // public static bool TryGetBufferIndex<T>(BufferLookup<T> lookup, BufferLookup<PlayerContainers> containers, Entity player, SlotPosition slotPosition, out T? inventorySlot, out int bufferIndex)  where T : unmanaged,IBufferElementData,IGetSlot
    // {
    //     var container = GetPlayerContainer(containers, player, slotPosition.containerIndex);
    //     if(container.HasValue)
    //         return TryGetBufferIndex<T>(lookup, slotPosition.slotIndex, container.Value.entity, out inventorySlot, out bufferIndex);
    //     inventorySlot = null;
    //     bufferIndex = -1;
    //     return false;
    // }


    public static bool TryGetBufferIndex(BufferLookup<InventorySlot> slotsLookup, int slotIndex, Entity container, out int itemID, out int bufferIndex)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            var slot = slots[j];
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
            var slot = slots[j];
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
    public static bool BufferContains(BufferLookup<InventorySlot> lookup, BufferLookup<EntityContainers> containers, Entity player, SlotPosition pos)
    {
        var contaner = GetContainer(containers, player, pos.containerIndex);
        if (contaner.HasValue)
            return BufferContains(lookup, contaner.Value.entity, pos.slotIndex);
        return false;
    }

    public static EquipmentEvent[] ClearContainer(EntityCommandBuffer ecb,ComponentLookup<ContainerComponent> componentLookup,BufferLookup<LinkedContainers> linked,BufferLookup<ItemBarData> bars,BufferLookup<InventorySlot> lookup, BufferLookup<EntityContainers> containers, Entity player, int containerIndex)
    {
        var container = GetContainer(containers, player, containerIndex);
        if (container.HasValue)
        {       
            NewItemInTheSlot(ecb,container.Value.entity,componentLookup[container.Value.entity],player,lookup);          
            lookup[container.Value.entity].Clear();
            bars[container.Value.entity].Clear();
            linked[container.Value.entity].Clear();
            return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(0, 2), containerIndex) };
        }
        return null;
    }
    public static EquipmentEvent[] ClearAllContainer(EntityCommandBuffer ecb,ComponentLookup<ContainerComponent> componentLookup,BufferLookup<LinkedContainers> linked ,BufferLookup<ItemBarData> bars,BufferLookup<InventorySlot> lookup, BufferLookup<EntityContainers> containers, Entity player)
    {
        var playerContainers = containers[player];
        foreach (var container in playerContainers)
        {
            NewItemInTheSlot(ecb,container.entity,componentLookup[container.entity],player,lookup);  
            lookup[container.entity].Clear();
            bars[container.entity].Clear();
            linked[container.entity].Clear();
        }
        return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(0, 3), 0) };
    }
    public static List<InventorySlot> TryGetAllItemsInContainer(BufferLookup<InventorySlot> slotsLookup, BufferLookup<EntityContainers> containers, Entity player, int containerIndex, int itemID)
    {
        var container = GetContainer(containers, player, containerIndex);
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


    public static EquipmentEvent[] MoveBetweenContainers(ref SystemState state,ref EntityCommandBuffer ecb,BufferLookup<LinkedContainers> linked, BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, Entity connection, Entity player, EntityContainers containersFrom, EntityContainers containersTo,
    int slotTo, int slotFrom, int value = int.MaxValue,bool eventForSlotFrom = false,bool moveBetweenObjects = false, bool serverMove = false)
    { 
        return MoveBetweenContainers(ref state,ref ecb,linked,barsLookup,slotsLookup,connection,player,containersFrom,containersTo,slotTo,slotFrom,value,out int c,eventForSlotFrom,moveBetweenObjects,serverMove);
    }
    public static EquipmentEvent[] MoveBetweenContainers(ref SystemState state,ref EntityCommandBuffer ecb, BufferLookup<LinkedContainers> linked, BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, Entity connection,Entity player, EntityContainers containersFrom, EntityContainers containersTo,
    int slotTo, int slotFrom, int value, out int transferValue, bool eventForSlotFrom = false,bool moveBetweenObjects = false, bool serverMove = false)
    {
        TryGetBufferIndex(slotsLookup, slotFrom, containersFrom.entity, out InventorySlot? itemFrom, out int fromIndex);
        TryGetBufferIndex(slotsLookup, slotTo, containersTo.entity, out InventorySlot? itemTo, out int toIndex);
        bool haveBarFrom =  TryGetBufferIndex(barsLookup, slotFrom, containersFrom.entity, out var barFrom, out int barFromIndex);
        bool haveBarTo = TryGetBufferIndex(barsLookup, slotTo, containersTo.entity, out var barTo, out int barToIndex);
        
        bool haveLinkedFrom =  TryGetBufferIndex(linked, slotFrom, containersFrom.entity, out var linkedFrom, out int linkedFromIndex);
        bool haveLinkedTo = TryGetBufferIndex(linked, slotTo, containersTo.entity, out var linkedTo, out int linkedToIndex);

        transferValue = 0;
        if (!itemFrom.HasValue) return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(slotTo, 1), containersTo.index) };
        
        int number;
        int stackMax = ItemsAsset.instance.GetStackMax(itemFrom.Value.itemId);
        var containerCompTo = state.EntityManager.GetComponentData<ContainerComponent>(containersTo.entity);
        var containerCompFrom = state.EntityManager.GetComponentData<ContainerComponent>(containersFrom.entity);

        int startToIndex = toIndex; 


        Debug.Log("dziala  " + fromIndex + " " + toIndex);
        if (CheckRequirements(containerCompTo, itemFrom.Value.itemId,serverMove))
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
                        if(haveLinkedTo)
                            linked[containersTo.entity].ElementAt(linkedToIndex).slot = slot.slot; 


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
                        if (!moveBetweenObjects && containersFrom.index == containersTo.index)
                        {
                            slot.slot = slotFrom;
                            if(haveBarTo)
                                barsLookup[containersTo.entity].ElementAt(barToIndex).slot = slot.slot;
                            if(haveLinkedTo)
                                linked[containersTo.entity].ElementAt(linkedToIndex).slot = slot.slot; 
                        }
                        else
                        {
                            fromBuffer.Add(new InventorySlot()
                            {
                                slot = slotFrom,
                                itemId = itemTo.Value.itemId,
                                quantity = itemTo.Value.quantity,
                                quality = itemTo.Value.quality,
                                wetness = itemTo.Value.wetness,
                                color = itemTo.Value.color
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

                            if(haveLinkedTo)
                            {
                                linked[containersFrom.entity].Add(new LinkedContainers()
                                {
                                   slot = slotFrom,
                                   containerEntity = linkedTo.Value.containerEntity,
                                   containerIndex = linkedTo.Value.containerIndex
                                });
                            }
                        }
                    }

                    toIndex = -1;
                    value = itemFrom.Value.quantity;
                }

                NewItemInTheSlot(ecb, containerCompTo,slotTo,player,itemTo);
                if(slotFrom >= 0)NewItemInTheSlot(ecb, containerCompFrom,slotFrom ,player,itemFrom);


                

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
                        if (haveLinkedFrom) linked[containersFrom.entity].RemoveAtSwapBack(linkedFromIndex);
                    }
                }
                else
                {
                    if (containersFrom.index == containersTo.index && number <= 0)
                    {
                        from.slot = slotTo;
                        if(haveBarFrom) barsLookup[containersFrom.entity].ElementAt(barFromIndex).slot = slotTo;
                        if(haveLinkedFrom) linked[containersFrom.entity].ElementAt(linkedFromIndex).slot = slotTo;
                    }
                    else
                    { 
                        toBuffer.Add(new InventorySlot()
                        {
                            slot = slotTo,
                            itemId = itemFrom.Value.itemId,
                            quantity = to,
                            quality = itemFrom.Value.quality,
                            wetness = itemFrom.Value.wetness,
                            color = itemFrom.Value.color
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

                        if(haveLinkedFrom)
                        {
                            linked[containersTo.entity].Add( new LinkedContainers()
                            {
                                slot = slotTo,
                                containerEntity = linkedFrom.Value.containerEntity,
                                containerIndex = linkedFrom.Value.containerIndex
                            });
                        }

                        if (number <= 0)
                        {
                            fromBuffer.RemoveAtSwapBack(fromIndex);
                            if (haveBarFrom) barsLookup[containersFrom.entity].RemoveAtSwapBack(barFromIndex);
                            if (haveLinkedFrom) linked[containersFrom.entity].RemoveAtSwapBack(linkedFromIndex);

                            if(slotFrom >= 0 && startToIndex >= 0)
                            {
                                toBuffer.RemoveAtSwapBack(startToIndex);
                                if (haveBarTo) barsLookup[containersTo.entity].RemoveAtSwapBack(barToIndex);
                                if(haveLinkedTo) linked[containersTo.entity].RemoveAtSwapBack(linkedToIndex);
                            }
                        }
                    }
                }

                // create events
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



    public static void NewItemInTheSlot(EntityCommandBuffer ecb,ContainerComponent containerComponent, int slotPos, Entity player, InventorySlot? oldSlot)
    {
        Debug.Log("check!! outfit " + containerComponent.containerIndex);
        if(containerComponent.containerType == ContainerType.Outfit)
        {
            Debug.Log("new outfit");
            EntityHelper.CreateEntityWithComponent(ecb, new EQOnEquip()
            {
                slotPosition = new SlotPosition(containerComponent.containerIndex,slotPos),
                player = player,
                oldInventorySlot = oldSlot.HasValue ? oldSlot.Value : InventorySlot.Empty
            });
        }
    }
    public static void NewItemInTheSlot(EntityCommandBuffer ecb,Entity container,ContainerComponent containerComponent, Entity player, BufferLookup<InventorySlot> slots)
    {
        if(containerComponent.containerType == ContainerType.Outfit)
        {
            for(int i = 0; i < containerComponent.capacity; i++)
            {
                TryGetBufferIndex(slots,i,container, out InventorySlot? obj,out int bufferindex);
                EntityHelper.CreateEntityWithComponent(ecb, new EQOnEquip()
                {
                    slotPosition = new SlotPosition(containerComponent.containerIndex,i),
                    player = player,
                    oldInventorySlot = obj.HasValue ? obj.Value : InventorySlot.Empty
                });
            }
        }
    }


    public static void SendEvents(ref EntityCommandBuffer entityCommandBuffer,int networkID, params EquipmentEvent[] events)
    {
        foreach (EquipmentEvent eventData in events)
        {
            eventData.SetNetworkID(networkID);
            EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData);
        }
    }




    public static void AppendToBuffer(BufferLookup<InventorySlot> slots,Entity container, InventorySlot template, int quantity = 1) 
    {
        if(slots.TryGetBuffer(container, out var bufferData))
        {
            template.quantity = quantity;
            template.slot = bufferData.Length; 
            bufferData.Add(template);
        }
    }


    public static void SendEvents(EntityCommandBuffer entityCommandBuffer,int networkID, params EquipmentEvent[] events)
    {
        foreach (EquipmentEvent eventData in events)
        {
            eventData.SetNetworkID(networkID);
            EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, eventData);
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
    
    public static void Rain(ref SystemState state,float intensity, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containersLookup, Entity player)
    {
        var containers = containersLookup[player];
        foreach (var container in containers)
        {
            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.entity);
            if(containerComponent.serverContainer) continue;
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
    public static EquipmentEvent[] AddItems(ref SystemState state,EntityCommandBuffer ecb,ref EntitiesReferences entitiesReferences,BufferLookup<LinkedContainers> linkedContainers,BufferLookup<ItemBarData> barsLookup,BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player, EQTransferData[] values, EQGiveItem itemData)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        bool hasBar = ItemsAsset.instance.TryGetBarValues(itemData.item.itemId, out float startValue, out float maxValue);
        bool hasLinkedContainer = ItemsAsset.instance.hasLinkedContainer(itemData.item.itemId,out int capacity,out MandatoryProperties mandatoryProperties, out int mandatoryData);      
        float barVal = maxValue * Math.Clamp(itemData.barValue, 0f, 1f);
        var value = state.EntityManager.GetComponentData<ContainerSettings>(player);


        foreach (EQTransferData item in values)
        {
            if (item.slotExist && TryGetPlayerContainer(containers,player,item.pos.containerIndex,out EntityContainers? container))
            {
                if (TryGetBufferIndex(slotLookup, item.pos.slotIndex, container.Value.entity, out int id, out int bufferIndex))
                {
                    ref var i = ref slotLookup[container.Value.entity].ElementAt(bufferIndex);

                    if(hasBar && TryGetBufferIndex(barsLookup, item.pos.slotIndex, container.Value.entity, out var barValue, out int barIndex))
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
            if (!item.slotExist &&  TryGetPlayerContainer(containers,player,item.pos.containerIndex,out EntityContainers? container))
            {             
                slotLookup[container.Value.entity].Add(new InventorySlot()
                {
                    itemId = itemData.item.itemId,
                    quantity = item.quantity,
                    slot = item.pos.slotIndex,
                    wetness = itemData.item.wetness,
                    quality = itemData.item.quality,
                    color = itemData.item.color,
                });

                if(hasBar)
                {
                    barsLookup[container.Value.entity].Add(new ItemBarData() {
                        slot = item.pos.slotIndex,
                        value = barVal,
                        maxValue = maxValue
                    });
                }

                if(hasLinkedContainer)
                {
                    Entity entity = GoInGameServerSystem.CreateNewContainer(ref state,player,ecb,ref entitiesReferences,new ContainerStats()
                    {
                        capacity = capacity,
                        containerIndex = value.nextTempIndex,
                        serverContainer = true,
                        waterResistance = 0,
                        mandatoryProperties = mandatoryProperties,
                        mandatoryData = mandatoryData
                    },null,container.Value.index);

                    linkedContainers[container.Value.entity].Add(new LinkedContainers()
                    {
                       slot = item.pos.slotIndex,
                       containerEntity = entity,
                       containerIndex = value.nextTempIndex
                    });

                    value.nextTempIndex += 1;
                }

                equipmentEvents.Add(new EquipmentEvent(new EquipmentEventData(item.pos.slotIndex, 1), container.Value.index));
            }
        }
        ecb.SetComponent(player,value);

        return equipmentEvents.ToArray();
    }
    public static EquipmentEvent[] MoveItems(ref SystemState state,ref EntityCommandBuffer ecb,BufferLookup<LinkedContainers> linked,BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotLookup,Entity connection, BufferLookup<EntityContainers> containers,SlotPosition from, Entity player, EQTransferData[] values)
    {
        List<EquipmentEvent> equipmentEvents = new List<EquipmentEvent>();
        if (values == null) return null;
        var containerFrom = GetContainer(containers, player, from.containerIndex);
        if(!containerFrom.HasValue) return null;

        foreach (EQTransferData item in values)
        {
            var containerTo = GetContainer(containers, player, item.pos.containerIndex);
            equipmentEvents.AddRange(MoveBetweenContainers(ref state,ref ecb,linked, barsLookup,slotLookup, connection,player, containerFrom.Value,containerTo.Value,item.pos.slotIndex,from.slotIndex,item.quantity,true));
        }

        return equipmentEvents.ToArray();
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player,int itemID,int quantity = 1, byte wetness = 0, Quality quality = Quality.none)
    {
        return FindSlotForItem(ref state, slotLookup, containers, player, new InventorySlot() {
            itemId = itemID,
            quantity = quantity,
            wetness = wetness,
            quality = quality
        });
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player, int itemID, int quantity = 1, byte wetness = 0, Quality quality = Quality.none, params int[] findIncontainers)
    {
        return FindSlotForItem(ref state, slotLookup, containers, player, new InventorySlot()
        {
            itemId = itemID,
            quantity = quantity,
            wetness = wetness,
            quality = quality
        },findIncontainers);
    }
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player, InventorySlot inventorySlot)
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
    public static EQTransferData[] FindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player, InventorySlot inventorySlot, params int[] findIncontainers)
    {
        var playerContainers = containers[player];
        int stackMax = ItemsAsset.instance.GetStackMax(inventorySlot.itemId);
        int numberSlots = (int)Math.Ceiling((float)inventorySlot.quantity / (float)stackMax);
        List<SlotPosition> freeSlots = new List<SlotPosition>();
        List<EQTransferData> moves = new List<EQTransferData>();

        for (int i = 0; i < findIncontainers.Length; i++)
        {
            EntityContainers? container = null;
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

    public static int GetNextFreeSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player, params int[] findIncontainers)
    {
        var playerContainers = containers[player];

        for (int i = 0; i < findIncontainers.Length; i++)
        {
            EntityContainers? container = null;
            for (int j = 0; j < playerContainers.Length; j++)
            {
                if (playerContainers[j].index == findIncontainers[i])
                    container = playerContainers[j];
            }
            if (!container.HasValue) continue;

            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.Value.entity);
            var slots = slotLookup[container.Value.entity];
            bool[] occupiedSlots = new bool[slots.Length + 1];

            for (int j = 0; j < slots.Length; j++)
            {
                var slot = slots[j];
                if(slot.slot >= 0 && slot.slot < occupiedSlots.Length)
                {
                    occupiedSlots[slot.slot] = true;
                }      
            }

            for (int j = 0; j < occupiedSlots.Length; j++)
            {
                if(occupiedSlots[j] == false)
                    return j;
            }
        }
        return -1;
    }
    
    
    


    public static InventorySlot[] ReadLinkedContainer(BufferLookup<InventorySlot> slotLookup,BufferLookup<LinkedContainers> linkedContainers, Entity container, int slotIndex)
    {
        return ReadLinkedContainer(slotLookup, linkedContainers, container, slotIndex, out Entity linkedContainerEntity);
    }
    public static InventorySlot[] ReadLinkedContainer(BufferLookup<InventorySlot> slotLookup,BufferLookup<LinkedContainers> linkedContainers, Entity container, int slotIndex, out Entity linkedContainerEntity)
    {
        Debug.Log("nullll " + slotIndex + "  " + container);
        if(TryGetBufferIndex(linkedContainers,slotIndex,container,out var element, out int bufferIndex))
        {
            linkedContainerEntity = element.Value.containerEntity;
            var slots = slotLookup[element.Value.containerEntity];
            InventorySlot[] result = new InventorySlot[slots.Length];
            for(int i = 0;i < slots.Length;i++)
                result[i] = slots[i];      
            return result;
        }
        linkedContainerEntity = Entity.Null;
        Debug.Log("nullllllllllllllll");
        return null;
    }

    public static int CountItemsInLinkedContainer(BufferLookup<InventorySlot> slotLookup,BufferLookup<LinkedContainers> linkedContainers, Entity container, int slotIndex)
    {
        return CountItemsInLinkedContainer(slotLookup,linkedContainers,container,slotIndex,out Entity entity);
    }
    public static int CountItemsInLinkedContainer(BufferLookup<InventorySlot> slotLookup,BufferLookup<LinkedContainers> linkedContainers, Entity container, int slotIndex,out Entity linkedContainerEntity)
    {
        int counter = 0;
        if(TryGetBufferIndex(linkedContainers,slotIndex,container,out var element, out int bufferIndex))
        {
            linkedContainerEntity = element.Value.containerEntity;
            var slots = slotLookup[element.Value.containerEntity];
            for(int i = 0;i < slots.Length;i++)
                counter += slots[i].quantity;  

            return counter;
        }
        else
        {
            linkedContainerEntity = Entity.Null;
        }
        return counter;
    }
    public static int CountItemsInLinkedContainer(BufferLookup<InventorySlot> slotLookup,BufferLookup<LinkedContainers> linkedContainers, Entity container, int slotIndex,out Entity linkedContainerEntity, out int firstItemID)
    {
        int counter = 0;
        firstItemID = -1;
        if(TryGetBufferIndex(linkedContainers,slotIndex,container,out var element, out int bufferIndex))
        {
            linkedContainerEntity = element.Value.containerEntity;
            var slots = slotLookup[element.Value.containerEntity];
            if(slots.Length > 0) 
                firstItemID = slots[0].itemId; 
            for(int i = 0;i < slots.Length;i++)
                    counter += slots[i].quantity;  
            return counter;
        }
        else
        {
            linkedContainerEntity = Entity.Null;
        }
        return counter;
    }




    public static InventorySlot[] TryFindItemWithTag_Aggregated(ref SystemState state,BufferLookup<InventorySlot> slotLookup,BufferLookup<EntityContainers> containers, Entity player,int tagID, out int counter)
    {
        var playerContainers = containers[player];

        counter = 0;
        Dictionary<int,InventorySlot> foundSlots = new Dictionary<int,InventorySlot>();

        for (int i = 0; i < playerContainers.Length; i++)
        {
            var container = playerContainers[i];
            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.entity);
            if(containerComponent.containerType != ContainerType.Standard) continue;
            
            if(CheckRequirementsTag(containerComponent,tagID, out bool AllItemsHaveTheTag))
            {
                var slots = slotLookup[container.entity];

                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    if (AllItemsHaveTheTag || ItemsAsset.instance.ItemHasTheTag(slot.itemId,tagID))
                    {
                        counter += slot.quantity;
                        if(foundSlots.ContainsKey(slot.itemId))
                        {
                            var temp = foundSlots[slot.itemId] ;
                            temp.quantity += slot.quantity;
                            foundSlots[slot.itemId] = temp;
                        }
                        else
                            foundSlots.Add(slot.itemId, slot);  
                    }
                }
            }
        }
        return foundSlots.OrderBy(kv => kv.Key).Select(x => x.Value).ToArray();
    }
    public static SlotData[] TryFindItemWithTag(ref SystemState state,BufferLookup<InventorySlot> slotLookup,BufferLookup<EntityContainers> containers, Entity player,int tagID,out InventorySlot[] aggregated, out int counter)
    {
        var playerContainers = containers[player];

        counter = 0;
        List<SlotData> foundSlots = new List<SlotData>();
        Dictionary<int,InventorySlot> aggregator = new Dictionary<int,InventorySlot>();

        
        for (int i = 0; i < playerContainers.Length; i++)
        {
            var container = playerContainers[i];
            var containerComponent = state.EntityManager.GetComponentData<ContainerComponent>(container.entity);
            if(containerComponent.containerType != ContainerType.Standard) continue;
            
            if(CheckRequirementsTag(containerComponent,tagID, out bool AllItemsHaveTheTag))
            {
                var slots = slotLookup[container.entity];

                for (int j = 0; j < slots.Length; j++)
                {
                    var slot = slots[j];
                    if (slot.slot < 0) continue;
                    if (AllItemsHaveTheTag || ItemsAsset.instance.ItemHasTheTag(slot.itemId,tagID))
                    {
                        counter += slot.quantity;
                        
                        foundSlots.Add(new SlotData(new EQTransferData(new SlotPosition(containerComponent.containerIndex,slot.slot),slot.quantity,true),slot.itemId));
                        
                        if(aggregator.ContainsKey(slot.itemId))
                        {
                            var temp = aggregator[slot.itemId] ;
                            temp.quantity += slot.quantity;
                            aggregator[slot.itemId] = temp;
                        }
                        else
                            aggregator.Add(slot.itemId, slot);  
                    }
                }

            }
        }
        foundSlots = foundSlots
            .OrderBy(x => x.transferData.pos.containerIndex)
            .ThenBy(x => x.transferData.pos.slotIndex)
            .ToList();     

        aggregated = aggregator.OrderBy(kv => kv.Key).Select(x => x.Value).ToArray();  

        return foundSlots.ToArray(); 
    }
   
   
   
   
   
    public static bool TryFindSlotForItem(ref SystemState state, BufferLookup<InventorySlot> slotLookup, BufferLookup<EntityContainers> containers, Entity player, InventorySlot inventorySlot,out EQTransferData[] data, params int[] findIncontainers)
    {
        data = FindSlotForItem(ref state,slotLookup ,containers,player,inventorySlot, findIncontainers);
        return data.Length > 0;
    }

    public static EquipmentEvent[] SubtractItem(BufferLookup<InventorySlot> slotLookup,BufferLookup<EntityContainers>containers,SlotPosition slotPosition,Entity player,int value = 1)
    {
        return SubtractItem(slotLookup,containers,slotPosition,player,out var template,value);
    }

    public static void SubtractItem(BufferLookup<InventorySlot> slotLookup,Entity container, int bufferIndex,out InventorySlot? template, int value = 1, bool removeAtSwapBack = true)
    {
        template = slotLookup[container][bufferIndex];
        if(template == null) return;

        if(template.Value.quantity > value)
            slotLookup[container].ElementAt(bufferIndex).quantity -= value;
        else
        {
            if(removeAtSwapBack)
                slotLookup[container].RemoveAtSwapBack(bufferIndex);
            else
                slotLookup[container].RemoveAt(bufferIndex);
        }
    }
    public static EquipmentEvent[] SubtractItem(BufferLookup<InventorySlot> slotLookup,BufferLookup<EntityContainers>containers,SlotPosition slotPosition,Entity player,out InventorySlot? template, int value = 1)
    {
        template = null;
        if(TryGetPlayerContainer(containers,player,slotPosition.containerIndex, out var playerContainer))
        {
            if(TryGetBufferIndex<InventorySlot>(slotLookup,slotPosition.slotIndex,playerContainer.Value.entity,out var slotItem, out int bufferIndex))
                SubtractItem(slotLookup,playerContainer.Value.entity,bufferIndex,out template,value);
        }
        return new EquipmentEvent[] { new EquipmentEvent(new EquipmentEventData(slotPosition.slotIndex, 1),slotPosition.containerIndex) };  
    }
    


    
    public static bool CheckRequirementsTag(ContainerComponent containerComponent, int tagID, out bool AllItemsHaveTheTag)
    {
        AllItemsHaveTheTag = false;
        return !containerComponent.serverContainer && CheckRequirementsTag(containerComponent.mandatoryProperties,containerComponent.mandatoryData,tagID, out AllItemsHaveTheTag); 
    }
    private static bool CheckRequirementsTag(MandatoryProperties mandatoryProperties, int mandatoryData, int tagID, out bool AllItemsHaveTheTag)
    {
        AllItemsHaveTheTag = false;
        switch (mandatoryProperties)
        {
            case MandatoryProperties.none:
                return true;
            case MandatoryProperties.tag:
                AllItemsHaveTheTag = true;
                return tagID == mandatoryData;
            case MandatoryProperties.item:
                return ItemsAsset.instance.ItemHasTheTag(mandatoryData,tagID);
        }
        return false;
    }

    public static bool CheckRequirements(ContainerComponent containerComponent, int itemID,bool serverMove = false)
    {
        return (serverMove || !containerComponent.serverContainer) && CheckRequirements(containerComponent.mandatoryProperties, containerComponent.mandatoryData, itemID);
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
    public static void Deselection(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer,BufferLookup<LinkedContainers> linked,BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, BufferLookup<EntityContainers> containers, Entity player,int networkID, Entity connection)
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
                    var container = GetContainer(containers, player, selectedSlot.Position.containerIndex);
                    if (container.HasValue)
                    {
                        var events = MoveBetweenContainers(ref state, ref entityCommandBuffer,linked, barsLookup, slotsLookup, connection,player, container.Value, container.Value, slotIndex, selectedSlot.Position.slotIndex, quantity, out int transferValue);
                        quantity -= transferValue;
                        SendEvents(ref entityCommandBuffer, networkID,events);
                    }
                }
                if (quantity > 0)
                {
                    var items = FindSlotForItem(ref state, slotsLookup, containers, player, slot.Value.itemId, quantity);
                    var events = MoveItems(ref state, ref entityCommandBuffer,linked,barsLookup, slotsLookup, connection, containers, selectedSlot.Position, player, items);
                    SendEvents(ref entityCommandBuffer, networkID, events);
                }
            }
        }
    }


    public static void Clone(SlotPosition from, SlotPosition to,BufferLookup<ItemBarData> barsLookup, BufferLookup<InventorySlot> slotsLookup, BufferLookup<EntityContainers> containersLookup, Entity player, out InventorySlot? newSlot, out ItemBarData? newBarData)
    {
        var containerFrom = GetPlayerContainer(containersLookup,player,from);
        var containerTo = GetPlayerContainer(containersLookup,player,to);

        if(containerFrom.HasValue && containerTo.HasValue)
        {
            Clone<InventorySlot>(slotsLookup,from,to,containerFrom.Value.entity,containerTo.Value.entity, out newSlot); 
            Clone<ItemBarData>(barsLookup,from,to,containerFrom.Value.entity,containerTo.Value.entity, out newBarData); 
        }
        else
        {
            newSlot = null;
            newBarData = null;   
        }
    }
    private static void Clone<T>(BufferLookup<T> lookup, SlotPosition from, SlotPosition to, Entity containerFrom, Entity containerTo, out T? newValue) where T : unmanaged,IBufferElementData,IGetSlot
    {
        bool hasFrom = TryGetBufferIndex(lookup,from.slotIndex,containerFrom,out var itemFrom,out int bufferFrom);
        bool hasTo = TryGetBufferIndex(lookup,to.slotIndex,containerTo,out var itemTo,out int bufferTo);
        newValue = null;
       
        if(hasFrom)
        {
            var slot = lookup[containerFrom][bufferFrom];
            slot.SetSlot(to.slotIndex);
            if(hasTo)
            {
                ref var slotTo = ref lookup[containerTo].ElementAt(bufferTo);
                slotTo = slot;
            }
            else
                lookup[containerTo].Add(slot);  
            newValue = slot;
        }
        else if(hasTo)
        {
            lookup[containerTo].RemoveAtSwapBack(bufferTo);   
        } 
    }
}