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

    struct Box
    {
        public Box(float2 pos, float2 size, float2 velocity)
        {
            this.pos = pos;
            this.size = size;
            this.velocity = velocity;
        }
        public float2 pos;
        public float2 size;
        public float2 velocity;
    };

    static int k = 0;
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
            LocalTransform tempTransform1 = transforms[i];
            float2 velocity1 = velocities[i].Value;
            Hitbox2D tempHitbox1 = hitboxes[i];

            int m = 0;
            float2 topLeft1 = 
                new float2(
                tempTransform1.Position.x - tempHitbox1.size.x * 0.5f,
                tempTransform1.Position.y + tempHitbox1.size.y * 0.5f
                );

            float2 totalNormal = float2.zero;
            float minCollisionTime = 1f;
            Box box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);


            k++;
            for (int j = i + 1; j < entityArray.Length; j++)
            {
                LocalTransform tempTransform2 = transforms[j];
                Hitbox2D tempHitbox2 = hitboxes[j];

                float2 topLeft2 =
                new float2(
                tempTransform2.Position.x - tempHitbox2.size.x * 0.5f,
                tempTransform2.Position.y + tempHitbox2.size.y * 0.5f
                );

                Box box2 = new Box(new float2(topLeft2.x, topLeft2.y),tempHitbox2.size, velocities[j].Value);
                float collisiontime = SweptAABB(box1, box2, out float normalx, out float normaly);

                float2 offset;

                if (collisiontime <= minCollisionTime)
                {
                    minCollisionTime = collisiontime;
                    m = j;
                    totalNormal += new float2(normalx, normaly);
                }
            }

            if (minCollisionTime < 1f)
            {
                if(i == 0)
                {
                    Debug.LogWarning(i + " kolizja" + m);
                }

                tempTransform1.Position.x += box1.velocity.x * minCollisionTime;
                tempTransform1.Position.y += box1.velocity.y * minCollisionTime;
                float remainingtime = 1.0f - minCollisionTime;
                velocity1.y *= remainingtime;
                velocity1.x *= remainingtime;

                if (totalNormal.x > 0)
                {
                    velocity1.x = 0.01f;
                }
                else if (totalNormal.x < 0)
                {
                    velocity1.x = -0.01f;
                }
                if (totalNormal.y > 0)
                {
                    velocity1.y = 0.01f;
                }
                else if (totalNormal.y < 0)
                {
                    velocity1.y = -0.01f;
                }
            }

            tempTransform1.Position += new float3(velocity1.x,velocity1.y,0);
            state.EntityManager.SetComponentData(entityArray[i], tempTransform1);
        }

        entityArray.Dispose();
        transforms.Dispose();
        velocities.Dispose();
        hitboxes.Dispose();
        collidingPairs.Dispose();
        mtvMax.Dispose();
    }
    private float SweptAABB(Box b1, Box b2, out float normalx, out float normaly)
    {
        float xInvEntry, yInvEntry;
        float xInvExit, yInvExit;
        normalx = 0;  
        normaly = 0;  

        if (b1.velocity.x > 0.0f)
        {
            xInvEntry = b2.pos.x - (b1.pos.x + b1.size.x);
            xInvExit = (b2.pos.x + b2.size.x) - b1.pos.x;
        }
        else
        {
            xInvEntry = (b2.pos.x + b2.size.x) - b1.pos.x;
            xInvExit = b2.pos.x - (b1.pos.x + b1.size.x);
        }

        if (b1.velocity.y > 0.0f)
        {
            yInvEntry = b2.pos.y - (b1.pos.y + b1.size.y);
            yInvExit = (b2.pos.y + b2.size.y) - b1.pos.y;
        }
        else
        {
            yInvEntry = (b2.pos.y + b2.size.y) - b1.pos.y;
            yInvExit = b2.pos.y - (b1.pos.y + b1.size.y);
        }

        float xEntry = (b1.velocity.x == 0.0f) ? -Mathf.Infinity : xInvEntry / b1.velocity.x;
        float xExit = (b1.velocity.x == 0.0f) ? Mathf.Infinity : xInvExit / b1.velocity.x;

        float yEntry = (b1.velocity.y == 0.0f) ? -Mathf.Infinity : yInvEntry / b1.velocity.y;
        float yExit = (b1.velocity.y == 0.0f) ? Mathf.Infinity : yInvExit / b1.velocity.y;

        float entryTime = Mathf.Max(xEntry, yEntry);
        float exitTime = Mathf.Min(xExit, yExit);

        Debug.Log(entryTime + " " + exitTime);
        if (entryTime <= exitTime && entryTime <= 1f && entryTime >= 0f && CheckCollision(xInvEntry, xInvExit, yInvEntry, yInvExit, xEntry, yEntry))
        {
            if (entryTime == xEntry)
            {
                normalx = (b1.velocity.x > 0) ? -1 : 1; 
            }
            else
            {
                normaly = (b1.velocity.y > 0) ? -1 : 1; 
            }
            return entryTime;
        }

        if (StaticAABB(b1, b2))
        {
            Debug.Log("Statyczna kolizja wykryta!");
            normalx = (b1.pos.x < b2.pos.x) ? -1 : 1;
            normaly = (b1.pos.y < b2.pos.y) ? -1 : 1;
            return 0; 
        }
        return 1;
    }
    private bool CheckCollision(float xInvEntry, float xInvExit, float yInvEntry, float yInvExit, float xEntry, float yEntry)
    {
        bool xCollision = math.abs(xEntry) != Mathf.Infinity;
        bool yCollision = math.abs(yEntry) != Mathf.Infinity;

        if (xCollision && yCollision)
        {
            return true;
        }
        else if (yCollision)
        {
            return MyTools.HaveOppositeSigns(xInvEntry, xInvExit);
        }
        else if (xCollision)
        {
            return MyTools.HaveOppositeSigns(yInvEntry, yInvExit);
        }
        return MyTools.HaveOppositeSigns(yInvEntry, yInvExit) && MyTools.HaveOppositeSigns(yInvEntry, yInvExit);
    }
    private bool StaticAABB(Box b1, Box b2)
    {
        return (b1.pos.x < b2.pos.x + b2.size.x &&
                b1.pos.x + b1.size.x > b2.pos.x &&
                b1.pos.y < b2.pos.y + b2.size.y &&
                b1.pos.y + b1.size.y > b2.pos.y);
    }

    private float3 GetMTV(Box b1, Box b2)
    {
        float2 min1 = new float2(b1.pos.x, b1.pos.y - b1.size.y);
        float2 max1 = new float2(b1.pos.x + b1.size.x, b1.pos.y);

        float2 min2 = new float2(b2.pos.x, b2.pos.y - b2.size.y);
        float2 max2 = new float2(b2.pos.x + b2.size.x, b2.pos.y);

        float overlapX = System.Math.Min(max1.x - min2.x, max2.x - min1.x);
        float overlapY = System.Math.Min(max1.y - min2.y, max2.y - min1.y);

        if (overlapX < overlapY)
            return new float3(overlapX * (min1.x < min2.x ? -1 : 1), 0, 0);
        else
            return new float3(0, overlapY * (min1.y < min2.y ? -1 : 1), 0);
    }
}
