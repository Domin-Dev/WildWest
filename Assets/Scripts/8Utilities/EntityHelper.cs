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


    public static void SpawnEntityPrefab(EntityCommandBuffer entityCommandBuffer, Entity prefab, float3 position, quaternion quaternion, NewParticles target = default)
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

    public static bool TryFindBuildingObject<T>(Entity chunk,int2 tilePosition, BufferLookup<T> objects,out T result,out int index) where T : unmanaged,IGetGlobalTilePosition,IBufferElementData
    {
        if(objects.TryGetBuffer(chunk,out var buffer))
        {
            return TryFindBuildingObject(tilePosition,buffer,out result,out index);
        }
        result = default;
        index = -1;
        return false;
    }

    public static bool TryFindBuildingObject<T>(int2 tilePosition,DynamicBuffer<T> buffer,out T result,out int index) where T : unmanaged,IGetGlobalTilePosition,IBufferElementData
    {
        for (int i = 0; i < buffer.Length;i++)
        {
            var element = buffer[i];
            if(element.GlobalTilePosition.x == tilePosition.x && element.GlobalTilePosition.y == tilePosition.y)
            {
                result = element;
                index = i;
                return true;
            }
        }
        result = default;
        index = -1;
        return false;
    }


}