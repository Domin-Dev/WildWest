using Unity.Burst;

using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(EquipmentSystemGroup), OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]


partial struct ContainerClientSystem : ISystem
{
    
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ContainerComponent>().WithNone<ContainerLoaded>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    
    public void OnUpdate(ref SystemState state)
    {
        
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ContainerComponent> containerComponent,RefRO<GhostOwner> containerOwner, Entity entity) in SystemAPI.Query<RefRO<ContainerComponent>,RefRO<GhostOwner>>().WithNone<ContainerLoaded>().WithEntityAccess())
        {
            if(SystemAPI.IsComponentEnabled<GhostOwnerIsLocal>(entity)) 
            {
                NewEquipmentManager.instance.LoadContainer(containerComponent.ValueRO,entity);
            }
                
            foreach ((RefRO<Player> player,RefRO<GhostOwner> owner, Entity e) in SystemAPI.Query<RefRO<Player>,RefRO<GhostOwner>>().WithAll<PlayerContainers>().WithEntityAccess())
            {
                if(owner.ValueRO.NetworkId == containerOwner.ValueRO.NetworkId)
                {
                    entityCommandBuffer.AppendToBuffer(e, new PlayerContainers() 
                    {
                        entity = entity,
                        index = containerComponent.ValueRO.containerIndex
                    });
                    break;
                }
            }

            entityCommandBuffer.AddComponent<ContainerLoaded>(entity);
        }   
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}
