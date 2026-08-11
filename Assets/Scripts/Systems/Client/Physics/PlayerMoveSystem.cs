
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;



[UpdateInGroup(typeof(CopyCommandBufferToInputSystemGroup),OrderLast = true)]
public partial struct PlayerMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {    

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId, NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        state.RequireForUpdate<ShootingConfig>();
        entityQueryBuilder.Dispose();

    }
    public void OnUpdate(ref SystemState state)
    {
        ShootingConfig shootingConfig = SystemAPI.GetSingleton<ShootingConfig>();
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
     
        if (state.World.Flags == WorldFlags.GameServer)
        {
            foreach (var (playerInputSync, playerInput, player, velocity,spread, entity)
                in SystemAPI.Query<RefRW<PlayerInputSync>, RefRW<PlayerInput>, RefRO<Player>, RefRW<PhysicsVelocity>,RefRW<PlayerActionSpread>>().WithAll<Simulate>().WithEntityAccess())
            {
                playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
                float2 dir = math.normalizesafe(playerInput.ValueRO.movementDirection);

                velocity.ValueRW.Linear = new float3(dir.x,dir.y,0) * player.ValueRO.speed;
                if(networkTime.IsFirstTimeFullyPredictingTick) 
                    spread.ValueRW.Spread = Mathf.Clamp(spread.ValueRO.Spread + math.lengthsq(velocity.ValueRW.Linear) * networkTime.SimulationStepBatchSize * 0.1f * shootingConfig.sensitivityPlayerMove,0,shootingConfig.maxSpread);

            }
        }
        else
        {
            foreach (var (playerInput, player, velocity,spread, entity)
            in SystemAPI.Query<RefRO<PlayerInput>, RefRO<Player>, RefRW<PhysicsVelocity>,RefRW<PlayerActionSpread>>().WithAll<Simulate, GhostOwnerIsLocal>().WithEntityAccess())
            {
                float2 dir = math.normalizesafe(playerInput.ValueRO.movementDirection);
                velocity.ValueRW.Linear = new float3(dir.x,dir.y,0) * player.ValueRO.speed;
                if(networkTime.IsFirstTimeFullyPredictingTick) 
                    spread.ValueRW.Spread = Mathf.Clamp(spread.ValueRO.Spread + math.lengthsq(velocity.ValueRW.Linear) *  networkTime.SimulationStepBatchSize * 0.1f * shootingConfig.sensitivityPlayerMove,0,shootingConfig.maxSpread);
            }
        }
    }
}

