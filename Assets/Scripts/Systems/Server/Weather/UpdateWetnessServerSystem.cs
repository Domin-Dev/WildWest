using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.Processors;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct UpdateWetnessServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;

    private int simulationTickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
     //   EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
    //        .WithAny<Rain>();

   //     state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
    //    entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRW<Rain> rain, Entity entity) in
        SystemAPI.Query<RefRW<Rain>>().WithEntityAccess())
        {
            playerContainersLookup.Update(ref state);
            slotsLookup.Update(ref state);
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

            if (rain.ValueRO.tick == NetworkTick.Invalid)
            {
                rain.ValueRW.tick = GetTick(currentTick);
                continue;
            } 
            else  if (currentTick.Equals(rain.ValueRO.tick) || currentTick.IsNewerThan(rain.ValueRO.tick))
            {

                foreach ((RefRO<LinkedCharacter> linked, RefRO<NetworkId> network) in
                    SystemAPI.Query<RefRO<LinkedCharacter>, RefRO<NetworkId>>())
                {

                    Entity player = linked.ValueRO.entity;
                    int networkID = network.ValueRO.Value;

                    EQHelper.Rain(ref state, rain.ValueRO.intensity,slotsLookup, playerContainersLookup,player);
                    EQHelper.SendEvents(ref entityCommandBuffer, networkID, new EquipmentEvent(new EquipmentEventData(0, 4),0));
                }
                rain.ValueRW.time -= 5;
                if (rain.ValueRO.time <= 0)
                    entityCommandBuffer.DestroyEntity(entity);
                else
                    rain.ValueRW.tick = GetTick(currentTick);
            }
 
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();

    }

    private NetworkTick GetTick(NetworkTick current)
    {
        uint lifetimeInTicks = (uint)(5 * simulationTickRate);
        var targetTick = current;
        targetTick.Add(lifetimeInTicks);
        return targetTick;
    }
}
