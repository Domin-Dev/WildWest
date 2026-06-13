using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public struct PlayerCleanUp : ICleanupComponentData
{
    public int networkID;
}


[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
partial struct NewPlayerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewPlayerTag>();
        state.RequireForUpdate<Players>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var players = SystemAPI.GetSingleton<Players>();
        

        foreach ((RefRO<Player> player,RefRW<PhysicsMass> mass, RefRW<PlayerLook> playerLook, Entity entity) in SystemAPI.Query<RefRO<Player>,RefRW<PhysicsMass>, RefRW<PlayerLook>>().WithAll<Simulate,NewPlayerTag>().WithEntityAccess())
        {
            mass.ValueRW.InverseInertia = float3.zero;
            if (!SystemAPI.HasBuffer<Child>(entity)) continue;

            Hands hands = new Hands() { rotated = true};
            Character character = new Character() { isMove = false };
            var children = SystemAPI.GetBuffer<Child>(entity);

            SetUpPlayer(ref children, ref state, ref hands, ref character);



            int networkID =  state.EntityManager.GetComponentData<GhostOwner>(entity).NetworkId;
            players.hashMap[networkID] = entity;
            entityCommandBuffer.AddComponent<PlayerCleanUp>(entity,new PlayerCleanUp(){ networkID = networkID});


            if(state.World.IsServer())
            {
                entityCommandBuffer.AddComponent(entity, new LastAction() { tick = NetworkTick.Invalid });
                PlayerSourceConnection connection = new PlayerSourceConnection();
                foreach ((NetworkId netId,Entity e) in SystemAPI.Query<NetworkId>().WithEntityAccess())
                {
                    if (netId.Value == networkID)
                    {
                        connection.value = e;
                        entityCommandBuffer.AddComponent(entity, connection);
                        break;
                    }
                }
            }
            else
            {
                Entity update = entityCommandBuffer.CreateEntity();             
                entityCommandBuffer.AddComponent(update, new LifeStatsChangedRPC());
                entityCommandBuffer.AddComponent(update, new PlayerStatsChangedRPC());
            }


            if (state.World.IsServer() && ClientServerBootstrap.HasClientWorlds)
            {
                if (ClientServerBootstrap.HasClientWorlds)
                {
                    SetName(ref children, ref state, string.Empty);
                    state.EntityManager.GetComponentObject<SpriteRenderer>(character.head).enabled = false;
                    state.EntityManager.GetComponentObject<SpriteRenderer>(hands.itemInMainHand).enabled = false;
                    state.EntityManager.GetComponentObject<SpriteRenderer>(hands.mainhand).enabled = false;
                    state.EntityManager.GetComponentObject<SpriteRenderer>(hands.sidehand).enabled = false;

                    foreach (var item in children)
                    {
                        if (state.EntityManager.HasComponent(item.Value, typeof(SpriteRenderer)))
                        {
                            state.EntityManager.GetComponentObject<SpriteRenderer>(item.Value).enabled = false;
                        }
                    }
                }
            }
            else
            {
                SetName(ref children, ref state, player.ValueRO.playerName.ToString());
                SetPlayerLook(ref playerLook.ValueRW, ref hands, ref character, ref state);
            }


            entityCommandBuffer.SetComponent(entity, character);
            entityCommandBuffer.SetComponent(entity, hands);
            entityCommandBuffer.RemoveComponent<NewPlayerTag>(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void SetPlayerLook(ref PlayerLook playerLook,ref Hands hands, ref Character character, ref SystemState state)
    {
        var mpb = new MaterialPropertyBlock();

        state.EntityManager.GetComponentObject<SpriteRenderer>(character.head).SetPropertyBlock(mpb);
        state.EntityManager.GetComponentObject<SpriteRenderer>(character.body).SetPropertyBlock(mpb);
        state.EntityManager.GetComponentObject<SpriteRenderer>(hands.mainhand).SetPropertyBlock(mpb);
        state.EntityManager.GetComponentObject<SpriteRenderer>(hands.sidehand).SetPropertyBlock(mpb);

        SetMaterialColor(ref state, character.head, "_SkinColor", playerLook.look.skinColor);
        SetMaterialColor(ref state, character.body, "_SkinColor", playerLook.look.skinColor);
        SetMaterialColor(ref state, hands.sidehand, "_Color", playerLook.look.skinColor);
        SetMaterialColor(ref state, hands.mainhand, "_Color", playerLook.look.skinColor);

        SetMaterialColor(ref state, character.body, "_UnderwearColor", playerLook.look.underwearColor);

        SetMaterialColor(ref state, character.head, "_HairColor", playerLook.look.hairColor);

        SetMaterialIndex(ref state, character.head, "_HairIndex", playerLook.look.hairIndex);
        SetMaterialIndex(ref state, character.head, "_BeardIndex", playerLook.look.beardndex);
        SetMaterialIndex(ref state, character.head, "_PaintingsIndex", playerLook.look.faceDetailsIndex);
    }

    private void SetMaterialColor(ref SystemState state, Entity entity ,string name, float3 color)
    {
        var sprite = state.EntityManager.GetComponentObject<SpriteRenderer>(entity);
        HeroEditor.SetMaterialColor(sprite,name,new Color(color.x,color.y,color.z));
    }
    private void SetMaterialIndex(ref SystemState state, Entity entity, string name, int index)
    {
        var sprite = state.EntityManager.GetComponentObject<SpriteRenderer>(entity);
        HeroEditor.SetMaterialInt(sprite, name, index);
    }

    private void SetName(ref DynamicBuffer<Child> children, ref SystemState state, string name)
    {
        foreach (var item in children)
        {
            if (state.EntityManager.HasComponent(item.Value, typeof(TextMesh)))
            {
                var textMesh = state.EntityManager.GetComponentObject<TextMesh>(item.Value);
                textMesh.text = name;
                break;
            }
        }
    }
    
    private void SetUpPlayer(ref DynamicBuffer<Child> children,ref SystemState state, ref Hands hands, ref Character character)
    {
        foreach (var child in children)
        {
            if (SystemAPI.HasComponent<Head>(child.Value))
            {
                var spr = SystemAPI.GetBuffer<Child>(child.Value)[0];
                character.headParent = child.Value;
                character.head = spr.Value;
            }
            else if (SystemAPI.HasComponent<Body>(child.Value))
            {
                character.body = child.Value;
            }
            else if (SystemAPI.HasComponent<MainHand>(child.Value))
            {
                hands.main = child.Value;
                hands.itemInMainHand = GetChild<ItemPointTag>(child.Value, 4, ref state);
                hands.mainhand = GetChild(child.Value, 1,ref state);

                hands.aimPoint = GetChild<AimPointTag>(child.Value, 4, ref state);
                hands.hitboxPoint = GetChild<ItemHitboxTag>(child.Value, 4, ref state);
                hands.reloadPoint = GetChild<ReloadPointTag>(child.Value, 4, ref state);
            }
            else if (SystemAPI.HasComponent<SideHand>(child.Value))
            {
                hands.side = child.Value;
                hands.sidehand = GetChild(child.Value, 1,ref state);
                hands.itemInSideHand = GetChild(child.Value, 2,ref state);
            }
        }
    }
    private Entity GetChild(Entity parent, int depth, ref SystemState state)
    {
        for (int i = 0; i < depth; i++)
        {
            parent = SystemAPI.GetBuffer<Child>(parent)[0].Value;
        }
        return parent;
    }

    private Entity GetChild<T>(Entity parent, int depth, ref SystemState state) where T : struct, IComponentData
    {
        for (int i = 0; i < depth - 1; i++)
        {
            parent = SystemAPI.GetBuffer<Child>(parent)[0].Value;
        }

        var childs = SystemAPI.GetBuffer<Child>(parent);
        for (int i = 0; i < childs.Length; i++)
        {
            if (state.EntityManager.HasComponent<T>(childs[i].Value))
            {
                return childs[i].Value;
            }
        }
        return parent;
    }
}