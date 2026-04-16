using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

public static class EntityHelper
{
    public static Entity CreateEntityWithComponent<T>(ref EntityCommandBuffer entityCommandBuffer, T component = default) where T : unmanaged, IComponentData
    {
        Entity entity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(entity, component); 
        return entity;
    }

    public static Entity CreateEntityWithBuffer<T>(ref EntityCommandBuffer entityCommandBuffer) where T : unmanaged, IBufferElementData
    {
        Entity entity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddBuffer<T>(entity);
        return entity;
    }


    public static Entity CreateEntityWithComponent<T>(EntityCommandBuffer entityCommandBuffer, T component = default) where T : unmanaged, IComponentData
    {
        Entity entity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(entity, component); 
        return entity;
    }

    public static Entity CreateEntityWithBuffer<T>(EntityCommandBuffer entityCommandBuffer) where T : unmanaged, IBufferElementData
    {
        Entity entity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddBuffer<T>(entity);
        return entity;
    }



    public static Entity CreateEntityWithComponent<T>(EntityManager entityManager, T component = default) where T : unmanaged, IComponentData
    {
        Entity entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, component);
        return entity;
    }


    public static float TicksToSeconds(int ticks)
    {
        return (float)ticks / NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }

    public static NetworkTick AddTime(NetworkTick tick, float seconds)
    {
        tick.Add((uint)(1 + (NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate * seconds)));
        return tick;
    }
    
    public static NetworkTick AddTime(NetworkTick tick, int ticks)
    {
        tick.Add((uint)(ticks));
        return tick;
    }


    public static void SpawnEntityPrefab(EntityCommandBuffer entityCommandBuffer, Entity prefab, float3 position, quaternion quaternion, NewParticles target)
    {
        Entity entity = entityCommandBuffer.Instantiate(prefab);
        position.z = position.y;
        LocalTransform localTransform = new LocalTransform()
        {
            Rotation = quaternion,
            Position = position,
            Scale = 1f
        };
        entityCommandBuffer.SetComponent(entity, localTransform);
        entityCommandBuffer.SetComponent(entity, target);
    }

}