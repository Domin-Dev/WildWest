using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using UnityEngine.XR;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct EventsAtTickClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;


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
        ecb.Playback(state.EntityManager);
    } 
}


