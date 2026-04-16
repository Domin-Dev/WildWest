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
    
    BufferLookup<LinkedContainers> linkedContaines;
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ContainerComponent>().WithNone<ContainerLoaded>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        linkedContaines = SystemAPI.GetBufferLookup<LinkedContainers>();
    }

    
    public void OnUpdate(ref SystemState state)
    {
        linkedContaines.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ContainerComponent> containerComponent,RefRO<GhostOwner> containerOwner, Entity entity) in SystemAPI.Query<RefRO<ContainerComponent>,RefRO<GhostOwner>>().WithNone<ContainerLoaded>().WithEntityAccess())
        {        
            bool found = false;
            foreach ((RefRO<Player> player,RefRO<GhostOwner> owner,DynamicBuffer<PlayerContainers> playerContainers, Entity e) in SystemAPI.Query<RefRO<Player>,RefRO<GhostOwner>,DynamicBuffer<PlayerContainers>>().WithNone<NewPlayerTag>().WithEntityAccess())
            {
                if(owner.ValueRO.NetworkId == containerOwner.ValueRO.NetworkId)
                {
                    if(containerComponent.ValueRO.parentContainerIndex >= 0)
                    {
                        for(int i = 0; i < playerContainers.Length;i++)
                        {
                            var element = playerContainers[i];
                            if(element.index == containerComponent.ValueRO.parentContainerIndex)
                            {
                                var buffer = linkedContaines[element.entity];
                                for(int k = 0; k < buffer.Length;k++)
                                {
                                    var item = buffer[k];
                                    if(item.containerIndex == containerComponent.ValueRO.containerIndex)
                                    {
                                        buffer.ElementAt(k).containerEntity = entity; 
                                        break;
                                    }
                                }                           
                                break;
                            }
                        }
                    }


                    playerContainers.Add(new PlayerContainers() 
                    {
                        entity = entity,
                        index = containerComponent.ValueRO.containerIndex
                    });

                    if(containerComponent.ValueRO.containerIndex == EquipmentConfig.itemInHand_ContainerIndex)
                        entityCommandBuffer.AddComponent<ContainersLoaded>(e);


                    found = true;
                    break;
                }
            }
            if(!found) continue;




            if(SystemAPI.IsComponentEnabled<GhostOwnerIsLocal>(entity)) 
            {
                NewEquipmentManager.instance.LoadContainer(containerComponent.ValueRO,entity);
            }
            entityCommandBuffer.AddComponent<ContainerLoaded>(entity);
        }   
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}
