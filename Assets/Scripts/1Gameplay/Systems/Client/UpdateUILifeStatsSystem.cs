using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct UpdateUILifeStatsSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<LifeStatsChangedRPC,PlayerStatsChangedRPC>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((LifeStatsChangedRPC message, Entity entity) in
        SystemAPI.Query<LifeStatsChangedRPC>().WithEntityAccess())
        {
            foreach ((Health health,Hunger hunger, Thirst thirst) in SystemAPI.Query<Health,Hunger,Thirst>()
            .WithAll<GhostOwnerIsLocal, Simulate>())
            {
                LifeStatsUI.Instance.UpdateHealth(health.Value, health.Max);
                LifeStatsUI.Instance.UpdateFood(hunger.Value, hunger.Max);
                LifeStatsUI.Instance.UpdateThirst(thirst.Value, thirst.Max);
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        foreach ((PlayerStatsChangedRPC message, Entity entity) in
        SystemAPI.Query<PlayerStatsChangedRPC>().WithEntityAccess())
        {
            foreach (Player player in SystemAPI.Query<Player>()
            .WithAll<GhostOwnerIsLocal, Simulate>())
            {
                PlayerStatsUI.UpdateStat("MovementSpeed",player.speed);
                PlayerStatsUI.UpdateStat("Aesthetic",player.aesthetic);
                PlayerStatsUI.UpdateStat("Armor",player.armor);
                PlayerStatsUI.UpdateStat("Insulation",player.insulation);
                PlayerStatsUI.UpdateStat("WaterResistance",player.waterResistance);
                PlayerStatsUI.UpdateStat("Wetness",player.wetness);
            }
            entityCommandBuffer.DestroyEntity(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
