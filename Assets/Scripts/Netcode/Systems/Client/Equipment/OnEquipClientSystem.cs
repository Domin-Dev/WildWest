
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using UnityEngine.InputSystem.Processors;
using Unity.VisualScripting;


[UpdateInGroup(typeof(EquipmentSystemGroup))]
[UpdateAfter(typeof(EquipmentClientSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct OnEquipClientSystem : ISystem
{
    private BufferLookup<InventorySlot> slots;
    private BufferLookup<PlayerContainers> playerContainersLookup;

    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<EQOnEquipClient>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slots = state.GetBufferLookup<InventorySlot>(true);
        playerContainersLookup = state.GetBufferLookup<PlayerContainers>();
    } 

    public void OnUpdate(ref SystemState state)
    { 
        slots.Update(ref state);
        playerContainersLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<EQOnEquipClient> onEquip , Entity entity) in SystemAPI.Query<RefRO<EQOnEquipClient>>().WithEntityAccess())
        {
             Debug.Log("jest rpc!!!!");
            foreach ((RefRW<Hands> hands,RefRW<Character> character, Entity player) in SystemAPI.Query<RefRW<Hands>,RefRW<Character>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
            {
                 Debug.Log("jest goot!!!!  " + onEquip.ValueRO.slotPosition);

                if(EQHelper.TryGetBufferIndex(slots,playerContainersLookup,player, onEquip.ValueRO.slotPosition, out InventorySlot? slot, out int bufferIndex))
                {
                    var tag = ItemsAsset.instance.GetTagType<GarmentTag>(slot.Value.itemId,out Item item);
                    if(tag != null)
                    { 
                        var sprite = state.EntityManager.GetComponentObject<SpriteRenderer>(character.ValueRO.head);  
                        Color? color = slot.Value.color.ConvertToUnityColor();
                        Debug.Log("dzial!!!!!!!!!!!!!!!!!!!!");
                        HeroEditor.SetMaterialTexture2D(sprite,tag.texturePropertyName, (item as Garment).texture);
                        if(color.HasValue)
                            HeroEditor.SetMaterialColor(sprite,tag.colorPropertyName,color.Value);
                    }
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }
        
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
