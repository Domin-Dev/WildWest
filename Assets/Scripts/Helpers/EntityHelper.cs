using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public static class EntityHelper
{ 
    public static Entity CreateEntityWithComponent<T>(ref EntityCommandBuffer entityCommandBuffer,T component = default) where T : unmanaged,IComponentData
    {
        Entity entity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(entity, component);
        return entity;  
    }
}
