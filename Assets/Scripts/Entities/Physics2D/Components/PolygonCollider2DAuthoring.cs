using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public class PolygonCollider2DAuthoring : MonoBehaviour
{
    [SerializeField] List<float2> points = new List<float2>();
    public class Baker : Baker<PolygonCollider2DAuthoring>
    {
        public override void Bake(PolygonCollider2DAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            var points = AddBuffer<PolygonCollider2DBuffer>(entity);

            foreach (float2 polygonPoint in authoring.points)
            {
                points.Add(new PolygonCollider2DBuffer
                {
                    point = polygonPoint
                });
            }
        }
    }
}

public struct PolygonCollider2DBuffer : IBufferElementData
{
    public float2 point;
}


