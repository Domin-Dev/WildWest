
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using UnityEngine.InputSystem.Processors;
using Unity.VisualScripting;
using UnityEngine.UI;


[UpdateInGroup(typeof(EquipmentSystemGroup))]
[UpdateAfter(typeof(EquipmentClientSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct OnEquipClientSystem : ISystem
{
    private BufferLookup<InventorySlot> slots;
    private BufferLookup<EntityContainers> playerContainersLookup;

    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<EQOnEquipClient>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slots = state.GetBufferLookup<InventorySlot>(true);
        playerContainersLookup = state.GetBufferLookup<EntityContainers>();
    } 

    public void OnUpdate(ref SystemState state)
    { 
        slots.Update(ref state);
        playerContainersLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<EQOnEquipClient> onEquip , Entity entity) in SystemAPI.Query<RefRO<EQOnEquipClient>>().WithEntityAccess())
        {
            foreach ((RefRO<Character> character, RefRO<GhostOwner> owner , Entity player) in SystemAPI.Query<RefRO<Character>,RefRO<GhostOwner>>().WithAll<Player>().WithEntityAccess())
            {
                if(owner.ValueRO.NetworkId == onEquip.ValueRO.ownerID)
                {               
                    EQHelper.TryGetBufferIndex(slots,playerContainersLookup,player, onEquip.ValueRO.slotPosition, out InventorySlot? slot, out int bufferIndex);

                    var containerStats = EquipmentConfig.GetContainer(onEquip.ValueRO.slotPosition.containerIndex);
                    var tag = ItemsAsset.instance.GetTag<GarmentTag>(containerStats.stats.mandatoryData);
                    if(tag != null)
                    { 
                        Texture2D texture = null;
                        Color? color = null;
                        if(slot.HasValue && ItemsAsset.instance.TryGetItem<Garment>(slot.Value.itemId,out var item))
                        {
                            color = slot.Value.color.ConvertToUnityColor();
                            texture = item.texture;
                        }

                        var sprite = state.EntityManager.GetComponentObject<SpriteRenderer>(character.ValueRO.GetPart(tag.bodyPart));  
                        HeroEditor.SetMaterialTexture2D(sprite,tag.texturePropertyName, texture);
                        HeroEditor.SetMaterialColor(sprite,tag.colorPropertyName,color.HasValue ? color.Value : Color.white);
                        EntityHelper.CreateEntityWithComponent<PlayerStatsChangedRPC>(entityCommandBuffer);
                        if(state.EntityManager.HasComponent<GhostOwnerIsLocal>(player))
                            Sounds.instance.PlayerSound(8);
                    } 
                    break;  
                }  
            }   
            entityCommandBuffer.DestroyEntity(entity);
        }
        
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
