using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;

public static class EntityHelper
{
    public static Entity CreateEntityWithComponent<T>(ref EntityCommandBuffer entityCommandBuffer, T component = default) where T : unmanaged, IComponentData
    {
        Entity entity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(entity, component);
        return entity;
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
    public static EquipmentEvent[] MoveBetweenContainers(BufferLookup<InventorySlot> slotsLookup,
     PlayerContainers containersFrom, PlayerContainers containersTo, int slotTo, int slotFrom, int value, bool eventForSlotFrom = false)
    {
        TryGetBufferIndex(slotsLookup, slotFrom, containersFrom.entity, out int itemIDFrom, out int fromIndex);
        TryGetBufferIndex(slotsLookup, slotTo, containersTo.entity, out int itemIDTo, out int toIndex);
        int number;

        if (fromIndex >= 0 && (itemIDFrom == itemIDTo || itemIDTo == -1))
        {
            var fromBuffer = slotsLookup[containersFrom.entity];
            var toBuffer = slotsLookup[containersTo.entity];

            ref InventorySlot from = ref fromBuffer.ElementAt(fromIndex);
            int to;

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
                    ItemId = itemIDFrom,
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
}