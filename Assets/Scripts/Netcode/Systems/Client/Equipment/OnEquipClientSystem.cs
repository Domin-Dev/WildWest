
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using UnityEngine.InputSystem.Processors;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct OnEquipClientSystem : ISystem
{
    private BufferLookup<InventorySlot> slots;
    private BufferLookup<PlayerContainers> playerContainersLookup;

    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<EQOnEquipRPC,ReceiveRpcCommandRequest>();

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
        foreach ((RefRO<EQOnEquipRPC> onEquip ,RefRO<ReceiveRpcCommandRequest>  command, Entity entity) in SystemAPI.Query<RefRO<EQOnEquipRPC>,RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {

             Debug.Log("jest rpc!!!!");
            foreach ((RefRW<Hands> hands,RefRW<Character> character, Entity player) in SystemAPI.Query<RefRW<Hands>,RefRW<Character>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
            {
                 Debug.Log("jest goot!!!!  " + onEquip.ValueRO.position);

                if(EQHelper.TryGetBufferIndex(slots,playerContainersLookup,player, onEquip.ValueRO.position, out InventorySlot? slot, out int bufferIndex))
                {
                    Debug.Log("jest goot!!!!");
                    var tag = ItemsAsset.instance.GetTagType<GarmentTag>(slot.Value.itemId,out Item item);
                    Debug.Log(tag.colorPropertyName + " tag!");
                    if(tag != null)
                    {
                        state.EntityManager.GetComponentObject<SpriteRenderer>(character.ValueRO.head);  
                        var sprite = state.EntityManager.GetComponentObject<SpriteRenderer>(entity);
                        Debug.Log("dzial!!!!!!!!!!!!!!!!!!!!");
                        HeroEditor.SetMaterialTexture2D(sprite,tag.texturePropertyName, (item as Garment).texture);
                    }
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }
        
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
