using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;




[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    private const float CellSize = 0.5f;



    readonly static int hitBoxLayer = 3;

    readonly static bool[,] collisionTab =
    //                  Buildings Players Bullets HitBox
    {                   // 0      1       2     3
     /*Buildings 0  */  { false, true  ,true  , false },
     /*Players   1  */  { true , false ,false , false },
     /*Bullets   2  */  { true , false ,false , true  },
     /*HitBox    3  */  { false, false ,true , false },
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

    ComponentLookup<IsChanged> isChanged;
    ComponentLookup<AlwaysUpdate> alwaysUpdate;

    ComponentLookup<Velocity2D> getVelocity;
    ComponentLookup<LocalTransform> getPosition;
    ComponentLookup<BoxCollider2D> getHitbox;
    ComponentLookup<Physics2D> getPhysics;
    ComponentLookup<Parent> getParent;


    BufferLookup<PhysicsChildrenBuffer> childrenBuffer;

    public void OnCreate(ref SystemState state)
    {
        entityMap = new NativeHashMap<int2, NativeList<Entity>>(100, Allocator.Persistent);
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId, NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();


        isChanged = SystemAPI.GetComponentLookup<IsChanged>(false);
        alwaysUpdate = SystemAPI.GetComponentLookup<AlwaysUpdate>();

        getVelocity = state.GetComponentLookup<Velocity2D>();
        getPosition = state.GetComponentLookup<LocalTransform>(false);
        getHitbox = state.GetComponentLookup<BoxCollider2D>();
        getPhysics = state.GetComponentLookup<Physics2D>();
        getParent = state.GetComponentLookup<Parent>(false);
        childrenBuffer = state.GetBufferLookup<PhysicsChildrenBuffer>(true);

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
        UpdateLookups(ref state);


        if (state.World.Flags == WorldFlags.GameServer)
        {
            foreach (var (playerInputSync, playerInput, player, velocity, entity)
                in SystemAPI.Query<RefRW<PlayerInputSync>, RefRW<PlayerInput>, RefRO<Player>, RefRW<Velocity2D>>().WithAll<Simulate>().WithEntityAccess())
            {
                playerInputSync.ValueRW.movementDir = playerInput.ValueRO.movementDirection;
                velocity.ValueRW.Value = playerInput.ValueRO.movementDirection * player.ValueRO.speed;
                bool shouldBeChanged = !(playerInput.ValueRO.movementDirection.x == 0 && playerInput.ValueRO.movementDirection.y == 0);
                state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
            }
        }
        else
        {
            foreach (var (playerInput, player, velocity, entity)
            in SystemAPI.Query<RefRO<PlayerInput>, RefRO<Player>, RefRW<Velocity2D>>().WithAll<Simulate, GhostOwnerIsLocal>().WithEntityAccess())
            {
                velocity.ValueRW.Value = playerInput.ValueRO.movementDirection * player.ValueRO.speed;
                bool shouldBeChanged = !(playerInput.ValueRO.movementDirection.x == 0 && playerInput.ValueRO.movementDirection.y == 0);
                state.EntityManager.SetComponentEnabled<IsChanged>(entity, shouldBeChanged);
            }
        }

        EntityQuery entities = SystemAPI.QueryBuilder().WithAll<Velocity2D, BoxCollider2D, LocalTransform, Physics2D, Simulate>().Build();


        NativeArray<Entity> entityArray = entities.ToEntityArray(Allocator.TempJob);
        NativeArray<LocalTransform> transforms = entities.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        NativeArray<Velocity2D> velocities = entities.ToComponentDataArray<Velocity2D>(Allocator.TempJob);
        NativeArray<BoxCollider2D> hitboxes = entities.ToComponentDataArray<BoxCollider2D>(Allocator.TempJob);
        NativeArray<Physics2D> physics = entities.ToComponentDataArray<Physics2D>(Allocator.TempJob);
        UpdateEntityMap(ref state, entityArray, physics, transforms);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        NativeHashMap<int, float> collisions = new NativeHashMap<int, float>(50, Allocator.TempJob);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];          
            if (!((isChanged.HasComponent(entity) && isChanged.IsComponentEnabled(entity)) || (alwaysUpdate.HasComponent(entity) && alwaysUpdate.IsComponentEnabled(entity))))
                continue;
            int layer = physics[i].layer;
            if (layer == 3) continue;


            BoxCollider2D tempHitbox1 = hitboxes[i];
            //  Debug.Log(entity);
            float3 tempTransform1 = GetWorldPosition(entity);
            float2 velocity1 = velocities[i].Value * SystemAPI.Time.DeltaTime;
            float2 topLeft1 =
                new float2(
                tempHitbox1.offset.x + tempTransform1.x - tempHitbox1.size.x * 0.5f,
                tempHitbox1.offset.y + tempTransform1.y + tempHitbox1.size.y * 0.5f
            );

            float3 vel = new float3(0, 0, 0);
            float3 pos = float3.zero;
            float3 offset = float3.zero;


            float minTime = float.MaxValue;
            int index = -1;
            float2 collision = float2.zero;
            k++;

            NativeList<Entity> potentialCollisions = GetPotentialCollisions(physics[i].cellIndex);
            bool destroy = false;

            for (int j = 0; j < potentialCollisions.Length; j++)
            {

                Entity entityToCheck = potentialCollisions[j];
                if (!SystemAPI.Exists(entityToCheck)) continue;
                if (entityToCheck == entity || !getPhysics.HasComponent(entityToCheck) || !collisionTab[getPhysics[entityToCheck].layer, layer]) continue;

                float3 tempTransform2 = GetWorldPosition(entityToCheck);
                BoxCollider2D tempHitbox2 = getHitbox[entityToCheck];

                float2 topLeft2 =
                new float2(
                 tempHitbox2.offset.x + tempTransform2.x - tempHitbox2.size.x * 0.5f,
                 tempHitbox2.offset.y + tempTransform2.y + tempHitbox2.size.y * 0.5f
                );

                Box box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);
                Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, getVelocity[entityToCheck].Value * SystemAPI.Time.DeltaTime);
                float collisiontime = SweptAABB(box1, box2, out float normalx, out float normaly);

                if (collisiontime < 1f)
                {
                    if (layer == 2 && BulletHit(ref state, ref entityCommandBuffer, entityToCheck, entity))
                    {     
                        destroy = true;
                        break;
                    }
                    collisions.Add(j, collisiontime);
                    if (collisiontime < minTime)
                    {
                        pos = tempTransform1;
                        vel = new float3(0, 0, 0);

                        minTime = collisiontime;
                        collision = new float2(normalx, normaly);
                        index = j;
                        pos.x = tempTransform1.x + box1.velocity.x * collisiontime;
                        pos.y = tempTransform1.y + box1.velocity.y * collisiontime;

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

            if (destroy)
            {
                entityCommandBuffer.AddComponent(entity, new DestroyEntityTag());
                entityCommandBuffer.RemoveComponent<Physics2D>(entity);
                continue;
            }
            else
            {
                if (minTime < 1f)
                {
                    topLeft1 =
                    new float2(
                    tempHitbox1.offset.x + pos.x - tempHitbox1.size.x * 0.5f,
                    tempHitbox1.offset.y + pos.y + tempHitbox1.size.y * 0.5f
                    );

                    tempTransform1 = pos;
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

                            float3 tempTransform2 = GetWorldPosition(entityToCheck);
                            BoxCollider2D tempHitbox2 = getHitbox[entityToCheck];

                            float2 topLeft2 =
                            new float2(
                            tempHitbox2.offset.x + tempTransform2.x - tempHitbox2.size.x * 0.5f,
                            tempHitbox2.offset.y + tempTransform2.y + tempHitbox2.size.y * 0.5f
                            );

                            box1 = new Box(new float2(topLeft1.x, topLeft1.y), tempHitbox1.size, velocity1);
                            Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, getVelocity[entityToCheck].Value * SystemAPI.Time.DeltaTime);
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
                                        pos = tempTransform1;
                                        vel = new float3(0, 0, 0);
                                    }
                                    minTime = collisiontime;
                                    pos.x = tempTransform1.x + box1.velocity.x * collisiontime;
                                    pos.y = tempTransform1.y + box1.velocity.y * collisiontime;



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

                        float3 tempTransform2 = GetWorldPosition(entityToCheck);
                        BoxCollider2D tempHitbox2 = getHitbox[entityToCheck];
                        float2 topLeft2 =
                        new float2(
                        tempHitbox2.offset.x + tempTransform2.x - tempHitbox2.size.x * 0.5f,
                        tempHitbox2.offset.y + tempTransform2.y + tempHitbox2.size.y * 0.5f
                        );
                        Box box2 = new Box(new float2(topLeft2.x, topLeft2.y), tempHitbox2.size, getVelocity[entityToCheck].Value * SystemAPI.Time.DeltaTime);
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

                    tempTransform1 = pos;
                    tempTransform1 += offset;
                    tempTransform1 += vel;
                }
                else
                    tempTransform1 += new float3(velocity1.x, velocity1.y, 0);
            }
            tempTransform1.z = tempTransform1.y;


            LocalTransform localTransform = getPosition[entity];
            localTransform.Position = tempTransform1;
            getPosition[entity] = localTransform;

            if (isChanged.HasComponent(entity))
            {
                isChanged.SetComponentEnabled(entity, false);
            }
            collisions.Clear();
            potentialCollisions.Dispose();
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
        //    Debug.Log("---------------------------------");
        physics.Dispose();
        collisions.Dispose();
        entityArray.Dispose();
        transforms.Dispose();
        velocities.Dispose();
        hitboxes.Dispose();
    }

    public float3 GetWorldPosition(Entity entity)
    {
        if (getParent.HasComponent(entity))
        {
            return getPosition[entity].Position + GetWorldPosition(getParent[entity].Value);
        }
        else
            return getPosition[entity].Position;
    }

    private bool BulletHit(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, Entity target, Entity bullet)
    {
        if (getPhysics[target].layer == hitBoxLayer)
        {

            Bullet bulletComponent = SystemAPI.GetComponent<Bullet>(bullet);
            Entity player = getParent[target].Value;
            if (SystemAPI.GetComponent<GhostOwner>(bullet).NetworkId != SystemAPI.GetComponent<GhostOwner>(player).NetworkId)
            {

                Debug.Log("Trafienie!!!!!!!!!" + " " + bullet + " " + state.World.Flags);

                if (state.World.IsServer())
                {
                    Health health = SystemAPI.GetComponent<Health>(player);
                    health.Value = math.clamp(health.Value - bulletComponent.damage, 0, health.Max);
                    entityCommandBuffer.SetComponent(player, health);
                    var connection = SystemAPI.GetComponent<PlayerSourceConnection>(player);
                    RPCHelper.SendRpc(ref entityCommandBuffer, connection.value, new LifeStatsChangedRPC());
                }
            }
            else
                return false;
        }
        return true;
    }

    private void UpdateLookups(ref SystemState state)
    {
        isChanged.Update(ref state);
        alwaysUpdate.Update(ref state);
        getVelocity.Update(ref state);
        getPosition.Update(ref state);
        getHitbox.Update(ref state);
        getPhysics.Update(ref state);
        getParent.Update(ref state);
        childrenBuffer.Update(ref state);
    }
    private void UpdateEntityMap(ref SystemState state,NativeArray<Entity> entityArray, NativeArray<Physics2D> physics, NativeArray<LocalTransform> transforms)
    {
        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            UpdateEntity(entity);
            if (childrenBuffer.HasBuffer(entity))
            {
                var buffer = childrenBuffer[entity];
                foreach (var child in buffer)
                {
                    UpdateEntity(child.LinkedEntity);
                }
            }
        }
    }
    private void UpdateEntity(Entity entity)
    {
        float3 position = GetWorldPosition(entity);
        int2 cellIndex = new int2((int)(position.x / CellSize), (int)(position.y / CellSize));
        Physics2D physics2D = getPhysics[entity];
        if (!physics2D.cellIndex.Equals(cellIndex))
        {
            SetValueInEntityMap(entity, physics2D.cellIndex, cellIndex);
            physics2D.cellIndex = cellIndex;
            getPhysics[entity] = physics2D;
        }
    }

    private void SetValueInEntityMap(Entity entity, int2 oldValue, int2 newValue)
    {
        if (oldValue.x != int.MinValue && oldValue.y != int.MinValue)
            RemoveValueInEntityMap(entity, oldValue);

        if (entityMap.ContainsKey(newValue))
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
            if (list.IsCreated && list.Contains(entity))
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
            int2 index = new int2((int)dir.x + gridIndex.x, (int)dir.y + gridIndex.y);

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
            xInvExit = (b2.pos.x + b2.size.x) - (b1.pos.x + b1.size.x);
        }
        else
        {
            xInvEntry = b1.pos.x - (b2.pos.x + b2.size.x);
            xInvExit = b1.pos.x - b2.pos.x;
        }

        if (b1.velocity.y >= 0.0f)
        {
            yInvEntry = (b2.pos.y - b2.size.y) - (b1.pos.y);
            yInvExit = b2.pos.y - b1.pos.y;
        }
        else
        {
            yInvEntry = (b1.pos.y - b1.size.y) - (b2.pos.y);
            yInvExit = (b1.pos.y - b1.size.y) - (b2.pos.y - b2.size.y);
        }


        float xEntry = (b1.velocity.x == 0.0f) ? -Mathf.Infinity : xInvEntry / math.abs(b1.velocity.x);
        float xExit = (b1.velocity.x == 0.0f) ? Mathf.Infinity : xInvExit / math.abs(b1.velocity.x);

        float yEntry = (b1.velocity.y == 0.0f) ? -Mathf.Infinity : yInvEntry / math.abs(b1.velocity.y);
        float yExit = (b1.velocity.y == 0.0f) ? Mathf.Infinity : yInvExit / math.abs(b1.velocity.y);

        float entryTime = Mathf.Max(xEntry, yEntry);
        float exitTime = Mathf.Min(xExit, yExit);

        bool isX = entryTime == xEntry;


        if (math.abs(entryTime) <= 1f && math.abs(entryTime) >= 0f
            && CheckCollision(xInvEntry, xInvExit, yInvEntry, yInvExit, isX, ref b1))
        {
            if (isX)
            {
                if (Mathf.Approximately(Mathf.Abs(yInvEntry), b2.size.y * 2) || Mathf.Approximately(Mathf.Abs(yInvExit), b2.size.y * 2))
                {
                    return 1;
                }
                normalx = (b1.velocity.x >= 0) ? -1 : 1;
            }
            else
            {
                if (Mathf.Approximately(Mathf.Abs(xInvEntry), b2.size.x * 2) || Mathf.Approximately(Mathf.Abs(xInvExit), b2.size.x * 2))
                {
                    return 1;
                }
                normaly = (b1.velocity.y >= 0) ? -1 : 1;
            }
            return math.abs(entryTime);
        }

        if (StaticAABB(b1, b2))
        {
            float2 normal = GetCollisionNormal(b1, b2);
            normalx = normal.x;
            normaly = normal.y;
            return 0;
        }

        return 1;
    }
    private bool CheckCollision(float xInvEntry, float xInvExit, float yInvEntry, float yInvExit, bool isX, ref Box box)
    {
        if (isX && (MyTools.HaveSameSigns(xInvEntry, xInvExit) || Equals(xInvEntry, 0)))
        {
            if (MyTools.HaveOppositeSigns(yInvEntry, yInvExit))
            {
                return !Equals(yInvEntry, 0);
            }
            else if ((yInvExit < 0 && IsGreaterThan(box.size.y, math.abs(yInvExit)) ||
                (yInvExit >= 0 && MyTools.HaveOppositeSigns(yInvEntry, yInvExit))))
            {
                return true;
            }
        }
        else if (!isX && (MyTools.HaveSameSigns(yInvEntry, yInvExit) || Equals(yInvEntry, 0)))
        {
            if (MyTools.HaveOppositeSigns(xInvEntry, xInvExit))
            {
                return !Equals(xInvEntry, 0);
            }
            else if (
                (xInvExit < 0 && IsGreaterThan(box.size.x, math.abs(xInvExit)))
                || (xInvExit >= 0 && MyTools.HaveOppositeSigns(xInvEntry, xInvExit)))
            {
                return true;
            }
        }

        return false;
    }
    private bool StaticAABB(Box b1, Box b2)
    {



        return IsGreaterThan(b2.pos.x + b2.size.x, b1.pos.x)
            && IsGreaterThan(b1.pos.x + b1.size.x, b2.pos.x)
            && IsGreaterThan(b2.pos.y, b1.pos.y - b1.size.y)
            && IsGreaterThan(b1.pos.y, b2.pos.y - b2.size.y);

        //return (b1.pos.x < b2.pos.x + b2.size.x &&
        //        b1.pos.x + b1.size.x > b2.pos.x &&
        //        b1.pos.y - b1.size.y < b2.pos.y  &&
        //        b1.pos.y > b2.pos.y - b2.size.y);
    }
    public bool IsGreaterThan(float a, float b, float epsilon = 1e-6f)
    {
        float c = a - b;
        return c >= epsilon;
    }
    public bool Equals(float a, float b, float epsilon = 1e-6f)
    {
        return Math.Abs(a - b) < epsilon;
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

        float[] tab = new float[4];

        tab[0] = Math.Abs(right1 - left2);
        tab[1] = Math.Abs(left1 - right2);
        tab[2] = Math.Abs(top1 - bottom2);
        tab[3] = Math.Abs(bottom1 - top2);

        float min = float.MaxValue;
        int index = 0;
        for (int i = 0; i < tab.Length; i++)
        {
            if (min > tab[i])
            {
                min = tab[i];
                index = i;
            }
        }


        switch (index)
        {
            case 0: return new float2(-1, 0);
            case 1: return new float2(1, 0);
            case 2: return new float2(0, -1);
            case 3: return new float2(0, 1);
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