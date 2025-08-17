using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public static class PolygonCollision
{
    public static bool ArePolygonsColliding(float2[] polyA, float2[] polyB)
    {
        return !HasSeparatingAxis(polyA, polyB) && !HasSeparatingAxis(polyB, polyA);
    }

    private static bool HasSeparatingAxis(float2 [] polyA, float2[] polyB)
    {
        for (int i = 0; i < polyA.Length; i++)
        {
            float2 p1 = polyA[i];
            float2 p2 = polyA[(i + 1) % polyA.Length];

            float2 axis = math.normalize(new float2(-(p2.y - p1.y), p2.x - p1.x));

            ProjectPolygon(axis, polyA, out float minA, out float maxA);
            ProjectPolygon(axis, polyB, out float minB, out float maxB);

            if (maxA < minB || maxB < minA)
                return true; 
        }
        return false;
    }

    private static void ProjectPolygon(float2 axis, float2[] poly, out float min, out float max)
    {
        float dot = Vector2.Dot(axis, poly[0]);
        min = max = dot;
        for (int i = 1; i < poly.Length; i++)
        {
            dot = Vector2.Dot(axis, poly[i]);
            if (dot < min) min = dot;
            if (dot > max) max = dot;
        }
    }
}
