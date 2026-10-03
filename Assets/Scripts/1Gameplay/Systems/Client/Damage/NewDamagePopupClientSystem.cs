using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(DamagePopupClientSystem))]
[RequireMatchingQueriesForUpdate]
partial struct NewDamagePopupClientSystem : ISystem
{
    readonly static Color normalColor = new Color(0.69f,0.16f,0.16f,1f);
    readonly static Color criticalColor = new Color(1f,0.0f,0.0f,1f);
    readonly static Color miningColor = new Color(0.2745f, 0.5098f, 0.7059f, 1f);
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<DamagePopup> damagePopup,EnabledRefRW<NewDamagePopup> popupTag,RefRO<LocalTransform> localTransform, Entity e) in SystemAPI.Query<RefRW<DamagePopup>,EnabledRefRW<NewDamagePopup>, RefRO<LocalTransform>>().WithAll<Simulate>().WithEntityAccess())
        {
            var textmesh = state.EntityManager.GetComponentObject<TextMesh>(e);
            textmesh.text = damagePopup.ValueRO.damageValue.ToString();
            textmesh.color = GetPopupColor(damagePopup.ValueRO.damageTag);
            damagePopup.ValueRW.startPosition = localTransform.ValueRO.Position;
            popupTag.ValueRW = false;
        }
    }


    private Color GetPopupColor(DamageTag damageTag)
    {
        switch (damageTag)
        {
            case DamageTag.Normal:
                return normalColor;
            case DamageTag.Critical:
                return criticalColor;
            case DamageTag.Mining:
                return miningColor;
        }
        return normalColor;
    }
}
