using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var entities = SystemAPI.QueryBuilder().WithAll<LocalTransform, Velocity2D, Hitbox2D>().Build();
        var collidingPairs = new NativeList<int2>(Allocator.TempJob);

        var entityArray = entities.ToEntityArray(Allocator.TempJob);
        var transforms = entities.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var velocities = entities.ToComponentDataArray<Velocity2D>(Allocator.TempJob);
        var hitboxes = entities.ToComponentDataArray<Hitbox2D>(Allocator.TempJob);

        NativeArray<float3> mtvMax = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);

        for (int i = 0; i < entityArray.Length; i++)
        {
            LocalTransform tempTransform = transforms[i];
            float2 velocity2D = velocities[i].Value;
            Hitbox2D tempHitbox = hitboxes[i];
            float2 min1, max1;
            float offsetX, offsetY;

            offsetX = tempHitbox.size.x * 0.5f;
            offsetY = tempHitbox.size.y * 0.5f;

            min1.x = tempTransform.Position.x + velocity2D.x - offsetX;
            min1.y = tempTransform.Position.y + velocity2D.y - offsetY;
            max1.x = tempTransform.Position.x + velocity2D.x + offsetX;
            max1.y = tempTransform.Position.y + velocity2D.y + offsetY;

            float3 largestMTV = float3.zero;

            for (int j = i + 1; j < entityArray.Length; j++)
            {
                float2 min2, max2;
                LocalTransform transform2 = transforms[j];
                Hitbox2D hitbox2 = hitboxes[j];
                float2 velocity2 = velocities[j].Value;

                offsetX = hitbox2.size.x * 0.5f;
                offsetY = hitbox2.size.y * 0.5f;

                min2.x = transform2.Position.x + velocity2.x - offsetX;
                min2.y = transform2.Position.y + velocity2.y - offsetY;
                max2.x = transform2.Position.x + velocity2.x + offsetX;
                max2.y = transform2.Position.y + velocity2.y + offsetY;

                if (CheckCollision(min1, max1, min2, max2))
                {
                    float3 offset = GetMTV(min1, max1, min2, max2);
                    collidingPairs.Add(new int2(i, j));

                    // Zamiast sumowaæ MTV, bierzemy ten o najwiêkszej wartoœci
                    if (math.abs(offset.x) > math.abs(largestMTV.x))
                    {
                        largestMTV.x = offset.x;
                    }
                    if (math.abs(offset.y) > math.abs(largestMTV.y))
                    {
                        largestMTV.y = offset.y;
                    }

                }
            }

            tempTransform.Position += largestMTV;
            tempTransform.Position += new float3(velocity2D.x, velocity2D.y, 0);
            state.EntityManager.SetComponentData(entityArray[i], tempTransform);
        }

        foreach (var pair in collidingPairs)
        {
          //  Debug.Log($"Collision detected between {entityArray[pair.x]} and {entityArray[pair.y]}");
        }

        entityArray.Dispose();
        transforms.Dispose();
        velocities.Dispose();
        hitboxes.Dispose();
        collidingPairs.Dispose();
        mtvMax.Dispose();
    }


    private bool CheckCollision(float2 min1, float2 max1 ,float2 min2 , float2 max2)
    {
        return (min1.x <= max2.x && max1.x >= min2.x) &&
               (min1.y <= max2.y && max1.y >= min2.y);
    }


    public float3 GetMTV(float2 min1, float2 max1, float2 min2, float2 max2)
    {
        float overlapX = System.Math.Min(max1.x - min2.x, max2.x - min1.x);
        float overlapY = System.Math.Min(max1.y - min2.y, max2.y - min1.y);

        if (overlapX < overlapY)
            return new float3(overlapX * (min1.x < min2.x ? -1 : 1), 0, 0);
        else
            return new float3(0, overlapY * (min1.y < min2.y ? -1 : 1), 0);
    }

    public bool SweptAABB(float2 min1, float2 max1, float2 velocity, float2 min2, float2 max2, out float tCollision)
    {
        float2 invEntry, invExit;
        float2 entry, exit;

        // Obliczamy moment wejœcia i wyjœcia na ka¿dej osi
        invEntry.x = (velocity.x > 0) ? (min2.x - max1.x) : (max2.x - min1.x);
        invExit.x = (velocity.x > 0) ? (max2.x - min1.x) : (min2.x - max1.x);

        invEntry.y = (velocity.y > 0) ? (min2.y - max1.y) : (max2.y - min1.y);
        invExit.y = (velocity.y > 0) ? (max2.y - min1.y) : (min2.y - max1.y);

        entry.x = (velocity.x == 0) ? float.NegativeInfinity : invEntry.x / velocity.x;
        exit.x = (velocity.x == 0) ? float.PositiveInfinity : invExit.x / velocity.x;

        entry.y = (velocity.y == 0) ? float.NegativeInfinity : invEntry.y / velocity.y;
        exit.y = (velocity.y == 0) ? float.PositiveInfinity : invExit.y / velocity.y;

        float entryTime = math.max(entry.x, entry.y);
        float exitTime = math.min(exit.x, exit.y);

        // Jeœli moment wejœcia jest póŸniejszy ni¿ moment wyjœcia, nie ma kolizji
        if (entryTime > exitTime || (entry.x < 0 && entry.y < 0) || entry.x > 1 || entry.y > 1)
        {
            tCollision = 1; // Brak kolizji
            return false;
        }

        // Kolizja nast¹pi³a
        tCollision = math.clamp(entryTime, 0, 1);
        return true;
    }

}
