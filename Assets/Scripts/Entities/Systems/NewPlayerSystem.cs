using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
partial struct NewPlayerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewPlayerTag>();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<Player> player, Entity entity) in SystemAPI.Query<RefRO<Player>>().WithAll<NewPlayerTag>().WithEntityAccess())
        {
            if (!SystemAPI.HasBuffer<Child>(entity)) continue;

            Hands hands = new Hands() { rotated = true };
            Character character = new Character() { isMove = false };
            var children = SystemAPI.GetBuffer<Child>(entity);

            SetUpPlayer(ref children, ref state, ref hands, ref character);

            if (state.World.Flags == WorldFlags.GameServer && ClientServerBootstrap.HasClientWorlds)
            {
                SetName(ref children, ref state, string.Empty);
                state.EntityManager.GetComponentObject<SpriteRenderer>(character.head).enabled = false;
                state.EntityManager.GetComponentObject<SpriteRenderer>(hands.itemInHand).enabled = false;
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
            else
            {
                SetName(ref children, ref state, player.ValueRO.playerName.ToString());
            }

            entityCommandBuffer.SetComponent(entity, hands);
            entityCommandBuffer.SetComponent(entity, character);
            entityCommandBuffer.RemoveComponent<NewPlayerTag>(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
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
                var spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(spr.Value);
                var mpb = new MaterialPropertyBlock();

                spriteRenderer.GetPropertyBlock(mpb);
                mpb.SetInt("_HairIndex", UnityEngine.Random.Range(14, 31));
                mpb.SetColor("_SkinColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_HairColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                // mpb.SetColor("_SkinColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                spriteRenderer.SetPropertyBlock(mpb);

                character.headParent = child.Value;
                character.head = spr.Value;
            }
            else if (SystemAPI.HasComponent<Body>(child.Value))
            {
                var spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(child.Value);
                var mpb = new MaterialPropertyBlock();
                spriteRenderer.GetPropertyBlock(mpb);
                mpb.SetColor("_SkinColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_OuterwearColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_UnderwearColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_PantsColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_ShirtColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_BeltColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_AccessoryColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetColor("_BagColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                mpb.SetInt("_Direction", 0);
                //character.directionHead
                spriteRenderer.SetPropertyBlock(mpb);

                character.body = child.Value;
            }
            else if (SystemAPI.HasComponent<MainHand>(child.Value))
            {
                hands.main = child.Value;
                hands.itemInHand = GetChild(child.Value, 4, ref state);
                hands.mainhand = GetChild(child.Value, 1,ref state);
            }
            else if (SystemAPI.HasComponent<SideHand>(child.Value))
            {
                hands.side = child.Value;
                hands.sidehand = GetChild(child.Value, 1,ref state);
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
}