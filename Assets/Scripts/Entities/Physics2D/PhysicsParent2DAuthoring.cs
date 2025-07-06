using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public struct PhysicsChildrenBuffer : IBufferElementData
{
    public Entity LinkedEntity;
}

public class PhysicsParent2DAuthoring : MonoBehaviour
{
    [SerializeField] List<Physics2DAuthoring> children = new List<Physics2DAuthoring>();
    public class Baker : Baker<PhysicsParent2DAuthoring>
    {
        public override void Bake(PhysicsParent2DAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            var physicsChildren = AddBuffer<PhysicsChildrenBuffer>(entity);

            foreach (var child in authoring.children)
            {
                if (child == null) continue;

                Entity childEntity = GetEntity(child, TransformUsageFlags.Dynamic);

                physicsChildren.Add(new PhysicsChildrenBuffer
                {
                    LinkedEntity = childEntity
                });
            }
        }
    }

}


