using NUnit.Framework.Interfaces;
using System;
using System.Numerics;
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
    private const float CellSize = 0.5f;

    readonly static bool[,] collisionTab =
    {         // 0      1
     /* 0 */   { true, true},
     /* 1 */   { true, true},
    };

    static int k = 0;
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

        public override string ToString()
        {
            return pos.ToString() + size.ToString() + velocity.ToString();  
        }
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityQuery entities = SystemAPI.QueryBuilder().WithAll<LocalTransform, Velocity2D, Hitbox2D,IsChanged,Physics2D>().Build();

        var entityArray = entities.ToEntityArray(Allocator.TempJob); 
        var transforms = entities.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var velocities = entities.ToComponentDataArray<Velocity2D>(Allocator.TempJob);
        var hitboxes = entities.ToComponentDataArray<Hitbox2D>(Allocator.TempJob);
        var physics = entities.ToComponentDataArray<Physics2D>(Allocator.TempJob);


        for (int i = 0; i < entityArray.Length; i++)
        {
            float2 position = transforms[i].Position.xy;
            int2 cellIndex = new int2((int)(position.x / CellSize), (int)(position.y / CellSize));
            Physics2D physics2D = physics[i];
          //  Debug.Log(cellIndex);
            physics2D.cellIndex = cellIndex;
            physics[i] = physics2D;
            state.EntityManager.SetComponentData(entityArray[i], physics2D);
        }


        NativeHashMap<int,float> collisions = new NativeHashMap<int,float>(20, Allocator.TempJob);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Hitbox2D tempHitbox1 = hitboxes[i];
            //if (!tempHitbox1.isChanged) continue;

          //  Debug.Log(tempHitbox1.isChanged);
            LocalTransform tempTransform1 = transforms[i];
            float2 velocity1 = velocities[i].Value; 
            float2 topLeft1 = 
                new float2(
                tempTransform1.Position.x - tempHitbox1.size.x * 0.5f,
                tempTransform1.Position.y + tempHitbox1.size.y * 0.5f
                );

            float3 vel = new float3(0,0,0);
            float3 pos = float3.zero;
            float3 offset = float3.zero;

            float minTime = float.MaxValue;
            int index = -1;
            float2 collision = float2.zero;
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

                Box box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);
                Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, velocities[j].Value);
                float collisiontime = SweptAABB(box1, box2, out float normalx, out float normaly);

                if (collisiontime < 1f )
                {
                    Debug.Log(collisiontime);
                    Debug.Log(normalx+" "+normaly);
                    collisions.Add(j, collisiontime);
                    if (collisiontime < minTime)
                    {      
                        pos = tempTransform1.Position;
                            vel = new float3(0, 0, 0);
                        
                        minTime = collisiontime;
                        collision = new float2(normalx, normaly);
                        index = j;
                        pos.x = tempTransform1.Position.x + box1.velocity.x * collisiontime;
                        pos.y = tempTransform1.Position.y + box1.velocity.y * collisiontime;


                        Debug.Log(velocity1);
                        float remainingtime = 1.0f - collisiontime;

                        float2 tempVel = float2.zero;
                       
                       
                        if (normalx == 0 || velocity1.x * normalx >= 0) tempVel.x = velocity1.x * remainingtime;
                        else tempVel.x = 0;

                        if (normaly == 0 || velocity1.y * normaly >= 0) tempVel.y = velocity1.y * remainingtime;
                        else tempVel.y = 0;
                       


                         Debug.Log(tempVel);
                        if (math.abs(tempVel.x) > math.abs(vel.x))
                        {
                            vel.x = tempVel.x;
                        }
                        if (math.abs(tempVel.y) > math.abs(vel.y))
                        {
                            vel.y = tempVel.y;
                        }
                        Debug.Log(k + " " + entityArray[j].Index + " " + normalx + " " + normaly + " " + collisiontime + " " + vel);
                    }
                }
            }

            if (minTime < 1f)
            {
                topLeft1 =
                new float2(
                pos.x - tempHitbox1.size.x * 0.5f,
                pos.y + tempHitbox1.size.y * 0.5f
                );

                 Debug.Log(minTime + " "+ pos + " "+ vel );

                tempTransform1.Position = pos;
                velocity1.x = vel.x;
                velocity1.y = vel.y;
                minTime = float.MaxValue;
                Box box1;
                bool s = true;
                if (vel.x != 0 || vel.y != 0)
                {
                    for (int j = i + 1; j < entityArray.Length; j++)
                    {
                        LocalTransform tempTransform2 = transforms[j];
                        Hitbox2D tempHitbox2 = hitboxes[j];

                        float2 topLeft2 =
                        new float2(
                        tempTransform2.Position.x - tempHitbox2.size.x * 0.5f,
                        tempTransform2.Position.y + tempHitbox2.size.y * 0.5f
                        );

                         box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);
                        Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, velocities[j].Value);
                        float collisiontime = SweptAABB(box1, box2, out float normalx, out float normaly);

                        if (collisiontime < 1f)
                        {
                            collisions.TryAdd(j, collisiontime);
                            if (collisiontime <= minTime && j != index && ((normalx != 0 && collision.x == 0) ||
                                (normaly != 0 && collision.y == 0)))
                            {
                                if (s)
                                {
                                    vel = float3.zero;
                                    s = false;
                                }
                                if (collisiontime < minTime)
                                {
                                    pos = tempTransform1.Position;
                                    vel = new float3(0, 0, 0);
                                }
                                minTime = collisiontime;
                                pos.x = tempTransform1.Position.x + box1.velocity.x * collisiontime;
                                pos.y = tempTransform1.Position.y + box1.velocity.y * collisiontime;


                                Debug.Log(velocity1);
                                float remainingtime = 1.0f - collisiontime;

                                float2 tempVel = float2.zero;

                                Box tempBox = box1;
                                tempBox.pos += box1.velocity * collisiontime;

                                float2 vector = CheckEdges(tempBox, box2);
                                Debug.Log(GetMTV(tempBox, box2));

                                if (normalx == 0 || velocity1.x * normalx >= 0) tempVel.x = velocity1.x * remainingtime;
                                else tempVel.x = 0;

                                if (normaly == 0 || velocity1.y * normaly >= 0) tempVel.y = velocity1.y * remainingtime;
                                else tempVel.y = 0;
                                


                                Debug.Log(tempVel);
                                if (math.abs(tempVel.x) > math.abs(vel.x))
                                {
                                    vel.x = tempVel.x;
                                }
                                if (math.abs(tempVel.y) > math.abs(vel.y))
                                {
                                    vel.y = tempVel.y;
                                }
                                Debug.Log(k + " " + entityArray[j].Index + " " + normalx + " " + normaly + " " + collisiontime + " " + vel);
                            }
                        }
                    }
                }

                box1 = new Box(new float2(topLeft1.x + vel.x, topLeft1.y + vel.y), tempHitbox1.size, velocity1);

                foreach (var item in collisions)
                {

                    // Debug.Log(item.Value);
                    LocalTransform tempTransform2 = transforms[item.Key];
                    Hitbox2D tempHitbox2 = hitboxes[item.Key];
                    float2 topLeft2 =
                    new float2(
                    tempTransform2.Position.x - tempHitbox2.size.x * 0.5f,
                    tempTransform2.Position.y + tempHitbox2.size.y * 0.5f
                    );
                    Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, velocities[item.Key].Value);
                    if (StaticAABB(box1, box2))
                    {
                        float3 localOffset = GetMTV(box1, box2);
                        if (math.abs(localOffset.x) > math.abs(offset.x))
                        {
                            offset.x = localOffset.x;
                        }
                        if (math.abs(localOffset.y) > math.abs(offset.y))
                        {
                            offset.y = localOffset.y;
                        }
                    }
                }

                tempTransform1.Position = pos;
                tempTransform1.Position += offset;
                tempTransform1.Position += vel;
            }
            else
                tempTransform1.Position += new float3(velocity1.x,velocity1.y,0);



            state.EntityManager.SetComponentData(entityArray[i], tempTransform1);   
            collisions.Clear();
        }

        physics.Dispose();
        collisions.Dispose();
        entityArray.Dispose();
        transforms.Dispose();
        velocities.Dispose();
        hitboxes.Dispose();
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
            Debug.Log("col");

            return entryTime;
        }

        if (StaticAABB(b1, b2))
        {
            float2 normal = GetCollisionNormal(b1, b2);
            normalx = normal.x;
            normaly = normal.y;
            Debug.Log(b1 + "\n" + b2);
            Debug.Log("stat");
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
    private float2 CheckEdges(Box b1, Box b2)
    {
        float2 min1 = new float2(b1.pos.x, b1.pos.y - b1.size.y);
        float2 max1 = new float2(b1.pos.x + b1.size.x, b1.pos.y);

        float2 min2 = new float2(b2.pos.x, b2.pos.y - b2.size.y);
        float2 max2 = new float2(b2.pos.x + b2.size.x, b2.pos.y);

        float overlapX = System.Math.Min(max1.x - min2.x, max2.x - min1.x);
        float overlapY = System.Math.Min(max1.y - min2.y, max2.y - min1.y);

        return new float2(overlapY >= 0 ? 1 : 0, overlapY >= 0 ? 1 : 0);
    }
    private float2 GetCollisionNormal(Box b1, Box b2)
    {
        float2 normal = new float2(1,1);

        float left1 = b1.pos.x;
        float right1 = b1.pos.x + b1.size.x;
        float bottom1 = b1.pos.y - b1.size.y;
        float top1 = b1.pos.y;

        float left2 = b2.pos.x;
        float right2 = b2.pos.x + b2.size.x;
        float bottom2 = b2.pos.y - b2.size.y;
        float top2 = b2.pos.y;

        if (Mathf.Approximately(right1, left2))
        {
            normal = new float2(-1, 0);  
        }
        else if (Mathf.Approximately(left1, right2))
        {
            normal = new float2(1, 0);
        }

        if (Mathf.Approximately(top1, bottom2))
        {
            normal = new float2(0, -1); 
        }
        else if (Mathf.Approximately(bottom1, top2))
        {
            normal = new float2(0, 1);
        }

        return normal;  
    }

}
