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
    public ItemSlot[] itemSlots;
}

public class NewEquipmentManager : MonoBehaviour
{

    public static NewEquipmentManager instance {  get; private set; }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containerParent;

    private Dictionary<int, Container> containers = new Dictionary<int, Container>();
    private bool open = false;

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
    public void LoadContainer(ContainerComponent containerComponent, Entity entity)
    {
        Container container = new Container();

        container.gridTransform = Instantiate(containerPrefab, containerParent).transform;
        container.entity = entity;
        container.gridIndex = containerComponent.containerIndex;
        container.itemSlots = new ItemSlot[containerComponent.capacity];

        var v = new EquipmentGrid(container.gridTransform, containerComponent.containerIndex);
        containers.Add(containerComponent.containerIndex, container);

        var slots = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(entity);
        foreach (InventorySlot slot in slots)
        {
            container.itemSlots[slot.slot] = new ItemSlot(slot);
        }


        UIManager.instance.LoadSlots(v, entity, containerComponent, v.gridIndex == 0);
        if (containerComponent.containerIndex == 0)
        {
            UIManager.instance.LoadBarSlots(containerComponent, entity);
           // ChangeSelectedSlot(0);
        }
    }

    public bool SlotIsEmpty(SlotPosition slotPosition)
    {
        return GetItemSlot(slotPosition) == null;
    }
    public void MoveItemData(SlotPosition from, SlotPosition to)
    {
        if(SlotIsEmpty(to))
        {
            ItemSlot fromData = GetItemSlot(from);
            ClearSlot(from);
            SetItemSlot(to, fromData);
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            RPCHelper.SendRpc(ref entityCommandBuffer, new EQMoveItem() { from = from, to = to });
            entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
            entityCommandBuffer.Dispose();
        }
        else
        {

        }
    }







    public void UpdateSlotIndex(SlotPosition slotPosition)
    {
        if(containers.TryGetValue(slotPosition.containerIndex,out Container container))
        {
            Debug.Log(slotPosition.containerIndex);
            ItemSlot itemSlot = LoadItemFromEntities(slotPosition);
            Debug.Log(itemSlot?.ToString() + ' ' + slotPosition);
            UIManager.instance.UpdateItemSlot(container, itemSlot,slotPosition.slotIndex);
        }
    }




    private ItemSlot GetItemSlot(SlotPosition slotPosition)
    {
        if (containers.TryGetValue(slotPosition.containerIndex, out Container container) && container.itemSlots.Length > slotPosition.slotIndex)
        {
            return container.itemSlots[slotPosition.slotIndex];
        }
        return null;
    }
    private void SetItemSlot(SlotPosition slotPosition, ItemSlot itemSlot)
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

  
    private ItemSlot LoadItemFromEntities(SlotPosition slotPosition)
    {
        Container container = containers[slotPosition.containerIndex];
        Debug.Log(ClientServerBootstrap.ClientWorld.EntityManager.GetComponentData<ContainerComponent>(container.entity).containerIndex);
        var buffer = ClientServerBootstrap.ClientWorld.EntityManager.GetBuffer<InventorySlot>(container.entity);
        foreach (var item in buffer)
        {
            Debug.Log(item.ItemId + " " + item.slot);
            if (item.slot == slotPosition.slotIndex)
            {
                ItemSlot slot = new ItemSlot(item);
                container.itemSlots[slotPosition.slotIndex] = slot;
                return slot;
            }
        }

        container.itemSlots[slotPosition.slotIndex] = null;
        return null;
    }
}
