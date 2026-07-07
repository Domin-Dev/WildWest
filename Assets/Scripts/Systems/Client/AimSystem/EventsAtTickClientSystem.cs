using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct EventsAtTickClientSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var prefabs = SystemAPI.GetSingleton<EntitiesReferences>();


        foreach ((EnabledRefRW<WaitForProcess> wait,RefRO<SystemEventData> eventData) in
        SystemAPI.Query<EnabledRefRW<WaitForProcess>,RefRO<SystemEventData>>())
        {
            if(!eventData.ValueRO.tick.IsNewerThan(tick))
            {
                wait.ValueRW = false;
            }          
        }


        foreach ((RefRO<PickUpItemCompletedClient> rpc, Entity entity) in
        SystemAPI.Query<RefRO<PickUpItemCompletedClient>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Sounds.instance.Click();
            UIManager.instance.NewCollectedItem(rpc.ValueRO.item.itemId,rpc.ValueRO.item.quantity);
            ecb.DestroyEntity(entity);
        }

        foreach ((RefRO<SpawnDamagePopup> rpc, Entity entity) in
        SystemAPI.Query<RefRO<SpawnDamagePopup>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            Entity popup = ecb.Instantiate(prefabs.worldTextEntity);
            ecb.SetComponent(popup, LocalTransform.FromPosition(new float3(rpc.ValueRO.position.x,rpc.ValueRO.position.y, -1)));
            ecb.SetComponent(popup, new DamagePopup()
            {
                lifetime = 1.5f,
                elapsedTime = 0,
                moveDirection = new float3(0, 0.4f, 0),
                damageTag = rpc.ValueRO.damageTag,
                damageValue = rpc.ValueRO.value
            });
            ecb.DestroyEntity(entity);
        }




        ecb.Playback(state.EntityManager);
    } 
}


