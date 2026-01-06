using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;

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




}