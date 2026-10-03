using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct CombineItemsServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<LinkedContainers> linkedLookup;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQDoubleClickAction>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedLookup.Update(ref state);
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQDoubleClickAction> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQDoubleClickAction>>().WithEntityAccess())
        {
            Entity connection = rpcCommandRequest.ValueRO.SourceConnection;
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(connection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(connection).Value;
            
            EquipmentEvent[] events = null;
            var container = EQHelper.GetContainer(playerContainersLookup, player, command.ValueRO.position.containerIndex);

            if(container.HasValue && !SystemAPI.HasComponent<ServerContainer>(container.Value.entity) && EQHelper.TryGetBufferIndex(slotsLookup, command.ValueRO.position.slotIndex,container.Value.entity, out InventorySlot? item, out int bufferIndex))           
            {
                if(EQHelper.TryFindContainerForItem(ref state,playerContainersLookup,player,out EntityContainers? newContainer,ContainerType.Outfit,item.Value.itemId))
                {
                    // try to find free slot or try combine the item in target container
                    if(EQHelper.TryFindSlotForItem(ref state, slotsLookup,playerContainersLookup, player,item.Value,out EQTransferData[] data, newContainer.Value.index))
                        events = EQHelper.MoveItems(ref state, ecb,linkedLookup,barsLookup,slotsLookup,connection,playerContainersLookup,command.ValueRO.position, player,data);
                    else
                        events = EQHelper.MoveBetweenContainers(ref state,ref ecb,linkedLookup,barsLookup,slotsLookup,connection,player,container.Value,newContainer.Value,0,command.ValueRO.position.slotIndex,int.MaxValue,true);
                }
                else if(!command.ValueRO.action)
                {
                    events = CombineItems(ref state, ref ecb, player,rpcCommandRequest.ValueRO.SourceConnection,container.Value,item.Value,bufferIndex,command.ValueRO.position.slotIndex);
                }
                
                EQHelper.SendEvents(ref ecb, networkID,command.ValueRO.position, events);
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


    /// <summary>
    /// Combine item in the same container.
    /// </summary>
    EquipmentEvent[] CombineItems(ref SystemState state,ref EntityCommandBuffer ecb,Entity player,Entity connection,EntityContainers container,InventorySlot item, int bufferIndex, int slotTo)
    {
        var slots = slotsLookup[container.entity];
        var element = slots.ElementAt(bufferIndex);
        int maxStack = ItemsAsset.instance.GetStackMax(item.itemId);
        List<EquipmentEvent> list = new List<EquipmentEvent>();
        List<(int from, int to, int amount)> moves = new();
        int foundMaxSlotPos = -1;
        int gap = maxStack - element.quantity;

        if (maxStack <= element.quantity) return null;            
        for (int i = 0; i < slots.Length; i++)
            {
                if (i == bufferIndex) continue;
                ref var slot = ref slots.ElementAt(i);
                if(slot.itemId == item.itemId && slot.quality == item.quality)
                {
                    if (slot.quantity == maxStack)
                    {
                        foundMaxSlotPos = slot.slot;
                        continue;
                    }

                    int transferValue = gap;
                    if (slot.quantity < gap)
                    {
                        transferValue = slot.quantity;
                        gap -= transferValue;
                    }
                    else
                        gap = 0;

                    moves.Add(new(slot.slot, slotTo, transferValue));
                    if (gap <= 0)
                        break;                     
                }
            }
        
        if(gap > 0 && foundMaxSlotPos >= 0)
            moves.Add(new(foundMaxSlotPos, slotTo, gap));

        foreach (var move in moves)
            list.AddRange(EQHelper.MoveBetweenContainers(ref state,ref ecb,linkedLookup,barsLookup, slotsLookup, connection,player, container, container, move.to, move.from, move.amount, true));

        return list.ToArray();
        
    }



}
