using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct UpdateItemInHandSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ItemInHandInputSync>();;
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (itemInHandInputSync, entity) in SystemAPI.Query<RefRO<ItemInHandInputSync>>().WithAll<Simulate>()
            .WithChangeFilter<ItemInHandInputSync>().WithNone<NewPlayerTag>().WithEntityAccess())
        {
            Debug.Log("Zmiana " + itemInHandInputSync.ValueRO.itemInHand);
            CharacterManager.instance.ChangeItemInHand(itemInHandInputSync.ValueRO.itemInHand, entity); 
        }
    }
}