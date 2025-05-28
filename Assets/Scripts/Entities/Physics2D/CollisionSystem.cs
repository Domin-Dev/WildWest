using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    private const float CellSize = 0.5f;


    // 2 - bullet
    readonly static bool[,] collisionTab =
    {         // 0      1       2
     /* 0 */   { false, true  ,true },
     /* 1 */   { true , false ,false },
     /* 2  */  { true , false ,false },
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

    NativeHashMap<int2, NativeList<Entity>> entityMap;

    public void OnCreate(ref SystemState state)
    {
        entityMap = new NativeHashMap<int2, NativeList<Entity>>(100, Allocator.Persistent);
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId,NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }
    public void OnDestroy(ref SystemState state)
    {
        if (entityMap.IsCreated)
        {
            foreach (var list in entityMap.GetValueArray(Allocator.Temp))
            {
                list.Dispose(); 
            }
            entityMap.Dispose();
        }
    }
    public void OnUpdate(ref SystemState state)
    {
        if (state.World.Flags == WorldFlags.GameServer)
        {
            foreach (var(playerInputSync, playerInput,player , velocity, entity)
                in SystemAPI.Query<RefRW<PlayerInputSync>,RefRW<PlayerInput>, RefRO<Player>, RefRW<Velocity2D>>().WithAll<Simulate>().WithEntityAccess())
            {
                playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
                velocity.ValueRW.Value = playerInput.ValueRO.movementDirection * SystemAPI.Time.DeltaTime * player.ValueRO.speed;
                bool shouldBeChanged = !(playerInput.ValueRO.movementDirection.x == 0 && playerInput.ValueRO.movementDirection.y == 0);
                state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
            }
        } 
        else
        {
            foreach (var (playerInput, player, velocity, entity)
            in SystemAPI.Query<RefRO<PlayerInput>, RefRO<Player>, RefRW<Velocity2D>>().WithAll<Simulate, GhostOwnerIsLocal>().WithEntityAccess())
            {
                velocity.ValueRW.Value = playerInput.ValueRO.movementDirection * SystemAPI.Time.DeltaTime * player.ValueRO.speed;
                bool shouldBeChanged = !(playerInput.ValueRO.movementDirection.x == 0 && playerInput.ValueRO.movementDirection.y == 0);
                state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
            }
        }

        EntityQuery entities = SystemAPI.QueryBuilder().WithAll<IsChanged,Velocity2D, BoxCollider2D,LocalTransform,Physics2D,Simulate>().Build();

        NativeArray<Entity> entityArray = entities.ToEntityArray(Allocator.TempJob);
        NativeArray<LocalTransform> transforms = entities.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        NativeArray<Velocity2D> velocities = entities.ToComponentDataArray<Velocity2D>(Allocator.TempJob);
        NativeArray<BoxCollider2D> hitboxes = entities.ToComponentDataArray<BoxCollider2D>(Allocator.TempJob);
        NativeArray<Physics2D> physics = entities.ToComponentDataArray<Physics2D>(Allocator.TempJob);
        UpdateEntityMap(ref state, entityArray, physics, transforms);


        var getVelocity = state.GetComponentLookup<Velocity2D>();
        var getPosition = state.GetComponentLookup<LocalTransform>();
        var getHitbox = state.GetComponentLookup<BoxCollider2D>();
        var getPhysics = state.GetComponentLookup<Physics2D>();

        NativeHashMap<int,float> collisions = new NativeHashMap<int,float>(50, Allocator.TempJob);

        for (int i = 0; i < entityArray.Length; i++)
        {

            BoxCollider2D tempHitbox1 = hitboxes[i];
            Entity entity = entityArray[i];
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

            int layer = physics[i].layer;
            float minTime = float.MaxValue;
            int index = -1;
            float2 collision = float2.zero;
            k++;

            NativeList<Entity> potentialCollisions = GetPotentialCollisions(physics[i].cellIndex);

       //     if (potentialCollisions.Length > 0) Debug.Log("<Color=#ffee00>Nowy update!!!" + state.World.Flags);
            for (int j = 0; j < potentialCollisions.Length; j++)
            {
                
                Entity entityToCheck = potentialCollisions[j];
                if (!SystemAPI.Exists(entityToCheck)) continue;
                if (entityToCheck == entity || !getPhysics.HasComponent(entityToCheck) || !collisionTab[getPhysics[entityToCheck].layer, layer]) continue;

                LocalTransform tempTransform2 = getPosition[entityToCheck];
                BoxCollider2D tempHitbox2 = getHitbox[entityToCheck];

                float2 topLeft2 =
                new float2(
                tempTransform2.Position.x - tempHitbox2.size.x * 0.5f,
                tempTransform2.Position.y + tempHitbox2.size.y * 0.5f
                );

                Box box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);
                Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, getVelocity[entityToCheck].Value);
                float collisiontime = SweptAABB(box1, box2, out float normalx, out float normaly);

                if (collisiontime < 1f)
                {
                    Debug.Log("@@@@@@@@@@ " + collisiontime + " " + normalx + " " + normaly);
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

                        float remainingtime = 1.0f - collisiontime;
                        float2 tempVel = float2.zero;
                        Box tempBox = box1;
                        tempBox.pos += box1.velocity * collisiontime;

                        if (normalx == 0 || velocity1.x * normalx >= 0) tempVel.x = velocity1.x * remainingtime;
                        else tempVel.x = 0;

                        if (normaly == 0 || velocity1.y * normaly >= 0) tempVel.y = velocity1.y * remainingtime;
                        else tempVel.y = 0;
                      
                        if (math.abs(tempVel.x) > math.abs(vel.x))
                        {
                            vel.x = tempVel.x;
                        }
                        if (math.abs(tempVel.y) > math.abs(vel.y))
                        {
                            vel.y = tempVel.y;
                        }
                        Debug.Log(vel);
                        // Debug.Log(k + " " + entityArray[j].Index + " " + normalx + " " + normaly + " " + collisiontime + " " + vel);
                    }
                }
            }



            if (minTime < 1f)
            {
                Debug.Log("s posiotion " + (pos - tempTransform1.Position).ToString());

                topLeft1 =
                new float2(
                pos.x - tempHitbox1.size.x * 0.5f,
                pos.y + tempHitbox1.size.y * 0.5f
                );

                tempTransform1.Position = pos;
                velocity1.x = vel.x;
                velocity1.y = vel.y;
                minTime = float.MaxValue;
                Box box1;
                bool s = true;
                if (vel.x != 0 || vel.y != 0)
                {
                    for (int j = i + 1; j < potentialCollisions.Length; j++)
                    {
                        Entity entityToCheck = potentialCollisions[j];
                        if (!state.EntityManager.Exists(entityToCheck) || entityToCheck == entity || !collisionTab[getPhysics[entityToCheck].layer, layer]) continue;


                        LocalTransform tempTransform2 = getPosition[entityToCheck];
                        BoxCollider2D tempHitbox2 = getHitbox[entityToCheck];

                        float2 topLeft2 =
                        new float2(
                        tempTransform2.Position.x - tempHitbox2.size.x * 0.5f,
                        tempTransform2.Position.y + tempHitbox2.size.y * 0.5f
                        );

                        box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);
                        Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, getVelocity[entityToCheck].Value);
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


            
                                float remainingtime = 1.0f - collisiontime;

                                float2 tempVel = float2.zero;

                                Box tempBox = box1;
                                tempBox.pos += box1.velocity * collisiontime;

                                if (normalx == 0 || velocity1.x * normalx >= 0) tempVel.x = velocity1.x * remainingtime;
                                else tempVel.x = 0;

                                if (normaly == 0 || velocity1.y * normaly >= 0) tempVel.y = velocity1.y * remainingtime;
                                else tempVel.y = 0;
                             

                          
                                if (math.abs(tempVel.x) > math.abs(vel.x))
                                {
                                    vel.x = tempVel.x;
                                }
                                if (math.abs(tempVel.y) > math.abs(vel.y))
                                {
                                    vel.y = tempVel.y;
                                }
                            }
                        }
                    }
                }

                box1 = new Box(new float2(topLeft1.x + vel.x, topLeft1.y + vel.y), tempHitbox1.size, velocity1);
                foreach (var item in collisions)
                {
                    Entity entityToCheck = potentialCollisions[item.Key];

                    LocalTransform tempTransform2 = getPosition[entityToCheck];
                    BoxCollider2D tempHitbox2 = getHitbox[entityToCheck];
                    float2 topLeft2 =
                    new float2(
                    tempTransform2.Position.x - tempHitbox2.size.x * 0.5f,
                    tempTransform2.Position.y + tempHitbox2.size.y * 0.5f
                    );
                    Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, getVelocity[entityToCheck].Value);
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

               // Debug.Log("new posiotion " + offset + vel);
                tempTransform1.Position = pos;
                tempTransform1.Position += offset;
                tempTransform1.Position += vel;
            }
            else
                tempTransform1.Position += new float3(velocity1.x,velocity1.y,0);

            float3 postion = tempTransform1.Position;
            postion.z = postion.y;
            tempTransform1.Position = postion;

            state.EntityManager.SetComponentData(entityArray[i], tempTransform1);
            state.EntityManager.SetComponentEnabled(entityArray[i],typeof(IsChanged), false);
            collisions.Clear();
            potentialCollisions.Dispose();
        }


        physics.Dispose();
        collisions.Dispose();
        entityArray.Dispose();
        transforms.Dispose();
        velocities.Dispose();
        hitboxes.Dispose();
    }





    private void UpdateEntityMap(ref SystemState state, NativeArray<Entity> entityArray, NativeArray<Physics2D> physics, NativeArray<LocalTransform> transforms)
    {
        for (int i = 0; i < entityArray.Length; i++)
        {
            float2 position = transforms[i].Position.xy;
            int2 cellIndex = new int2((int)(position.x / CellSize), (int)(position.y / CellSize));
            Physics2D physics2D = physics[i];
            if (!physics2D.cellIndex.Equals(cellIndex))
            {
                SetValueInEntityMap(entityArray[i], physics2D.cellIndex, cellIndex);
                physics2D.cellIndex = cellIndex;
                physics[i] = physics2D;
                state.EntityManager.SetComponentData(entityArray[i], physics2D);
            }
        }
    }
    private void SetValueInEntityMap(Entity entity,int2 oldValue ,int2 newValue)
    {
        if (oldValue.x != int.MinValue && oldValue.y != int.MinValue)
            RemoveValueInEntityMap(entity, oldValue);

        if(entityMap.ContainsKey(newValue))
        {
            entityMap[newValue].Add(entity);
        }
        else
        {
            var newList = new NativeList<Entity>(Allocator.Persistent) { entity };
            entityMap.Add(newValue, newList);
        }
    }
    private void RemoveValueInEntityMap(Entity entity, int2 oldValue)
    {
        if (entityMap.ContainsKey(oldValue))
        {
            var list = entityMap[oldValue];
            if(list.IsCreated && list.Contains(entity))
            {
                int index = GetIndex(list, entity);
                list.RemoveAtSwapBack(index);
                if (list.IsEmpty)
                {
                    list.Dispose();
                    entityMap.Remove(oldValue);
                }
            }
        }
    }  
    private int GetIndex(NativeList<Entity> list, Entity entity)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == entity)
            {
                return i;
            }
        }
        return -1;
    }
    private NativeList<Entity> GetPotentialCollisions(int2 gridIndex)
    {
        NativeList<Entity> entities = new NativeList<Entity>(Allocator.TempJob);

        if (entityMap.ContainsKey(gridIndex))
            entities.AddRange(entityMap[gridIndex].AsArray());
        for (int i = 0; i < 8; i++)
        {
            Vector2 dir = MyTools.directions8[i];
            int2 index = new int2((int)dir.x + gridIndex.x , (int)dir.y + gridIndex.y);

            if (entityMap.ContainsKey(index))
                entities.AddRange(entityMap[index].AsArray());
        }

        return entities;
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
            xInvExit = (b1.pos.x) - b2.pos.x;
        }

        if (b1.velocity.y > 0.0f)
        {
            yInvEntry = (b2.pos.y - b2.size.y) - (b1.pos.y);
            yInvExit = b2.pos.y  - b1.pos.y;
        }
        else
        {
            yInvEntry = (b2.pos.y) - (b1.pos.y - b1.size.y);
            yInvExit = (b2.pos.y - b2.size.y) - (b1.pos.y - b1.size.y);
        }



        float xEntry = (b1.velocity.x == 0.0f) ? -Mathf.Infinity : xInvEntry / b1.velocity.x;
        float xExit = (b1.velocity.x == 0.0f) ? Mathf.Infinity : xInvExit / b1.velocity.x;

        float yEntry = (b1.velocity.y == 0.0f) ? -Mathf.Infinity : yInvEntry / b1.velocity.y;
        float yExit = (b1.velocity.y == 0.0f) ? Mathf.Infinity : yInvExit / b1.velocity.y;

        float entryTime = Mathf.Max(xEntry, yEntry);
        float exitTime = Mathf.Min(xExit, yExit);

       // Debug.Log(b1 + "\n " + b2);
      //  Debug.Log($"{xInvEntry} {xInvExit} | {yInvEntry} {yInvExit} | {xEntry} {yEntry} ");
        if (entryTime <= exitTime && entryTime <= 1f && entryTime >= 0f && CheckCollision(xInvEntry, xInvExit, yInvEntry, yInvExit, xEntry, yEntry))
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            if (entryTime == xEntry)
            {
                if (Mathf.Approximately(Mathf.Abs(yInvEntry), b2.size.y*2) || Mathf.Approximately(Mathf.Abs(yInvExit), b2.size.y *2))
                {
                    return 1;
                }
                normalx = (b1.velocity.x > 0) ? -1 : 1; 
            }
            else
            {
                if (Mathf.Approximately(Mathf.Abs(xInvEntry), b2.size.x *2) || Mathf.Approximately(Mathf.Abs(xInvExit), b2.size.x * 2))
                {
                    return 1;
                }
                normaly = (b1.velocity.y > 0) ? -1 : 1; 
            }


            return entryTime;
        }

        if (StaticAABB(b1, b2))
        {
            float2 normal = GetCollisionNormal(b1, b2);
            normalx = normal.x;
            normaly = normal.y;
            Debug.Log("zero");
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
                b1.pos.y - b1.size.y < b2.pos.y  &&
                b1.pos.y > b2.pos.y - b2.size.y);
    }
    private float3 GetMTV(Box b1, Box b2)
    {
        //Debug.Log(b1);
      //  Debug.Log(b2);
        float2 min1 = new float2(b1.pos.x, b1.pos.y - b1.size.y);
        float2 max1 = new float2(b1.pos.x + b1.size.x, b1.pos.y);

        float2 min2 = new float2(b2.pos.x, b2.pos.y - b2.size.y);
        float2 max2 = new float2(b2.pos.x + b2.size.x, b2.pos.y);

        float overlapX = System.Math.Min(max1.x - min2.x, max2.x - min1.x);
        float overlapY = System.Math.Min(max1.y - min2.y, max2.y - min1.y);

        //Debug.Log(overlapY);
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
        float2 normal;

        float left1 = b1.pos.x;
        float right1 = b1.pos.x + b1.size.x;
        float bottom1 = b1.pos.y - b1.size.y;
        float top1 = b1.pos.y;

        float left2 = b2.pos.x;
        float right2 = b2.pos.x + b2.size.x;
        float bottom2 = b2.pos.y - b2.size.y;
        float top2 = b2.pos.y;

        Debug.Log(left1 + " " + right1 + " " + top1 + " " + bottom1);
        Debug.Log(left2 + " " + right2 + " " + top2+ " " + bottom2);

        float[] tab = new float[4];

        tab[0] = Math.Abs(right1 - left2);
        tab[1] = Math.Abs(left1 - right2);
        tab[2] = Math.Abs(top1 - bottom2);
        tab[3] = Math.Abs(bottom1 - top2);

        float min = float.MaxValue;
        int index = 0;
        for (int i = 0; i < tab.Length; i++)
        {
            if(min > tab[i])
            {
                min = tab[i];
                index = i;
            }
        }


        switch (index)
        {
            case 0: return new float2(-1, 0);
            case 1: return new float2(1, 0);
            case 2: return new float2(0,-1);
            case 3: return new float2(0,1);
            default: return float2.zero;
        }


        //if (Mathf.Approximately(right1, left2))
        //{
        //    normal = new float2(-1, 0);  
        //}
        //else if (Mathf.Approximately(left1, right2))
        //{
        //    normal = new float2(1, 0);
        //}

        //if (Mathf.Approximately(top1, bottom2))
        //{
        //    normal = new float2(0, -1); 
        //}
        //else if (Mathf.Approximately(bottom1, top2))
        //{
        //    normal = new float2(0, 1);
        //}  
    }

}
