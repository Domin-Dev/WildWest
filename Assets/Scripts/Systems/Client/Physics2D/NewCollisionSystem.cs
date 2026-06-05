
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct NewCollisionSystem : ISystem
{
        private const float CellSize = 0.5f;
    private const float DampingValue = 8f;
    private const float CleanupInterval = 90f;
    readonly static int hitBoxLayer = 3;
    readonly static bool[,] collisionTab =
    //                  Buildings Players Bullets HitBox WorldItem
    {                   // 0      1       2     3       4
     /*Buildings 0  */  { false, true  ,true  , false ,  false},
     /*Players   1  */  { true , false ,false , false ,  true},
     /*Bullets   2  */  { true , false ,false , true  ,  false},
     /*HitBox    3  */  { false, false ,true , false  ,  false},
     /*World Item 4 */  { false, true , false , false , false}
    };

    public static event Action<float2> onPlayerMove;

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

    NativeParallelMultiHashMap<int2, Entity> entityMap;

    ComponentLookup<IsChanged> isChanged;
    ComponentLookup<AlwaysUpdate> alwaysUpdate;

    ComponentLookup<Velocity2D> getVelocity;
    ComponentLookup<LocalTransform> getPosition;
    ComponentLookup<BoxCollider2D> getHitbox;
    ComponentLookup<Physics2D> getPhysics;
    ComponentLookup<ForceImpulse2D> getForceImpulse;
    ComponentLookup<Parent> getParent;
    BufferLookup<PhysicsChildrenBuffer> childrenBuffer;



    DynamicBuffer<LoadedChunks> loadedChunks;

    MapSettings map;


    private float deltaTime;
    private float timer;

    public void OnCreate(ref SystemState state)
    {
        
        entityMap = new NativeParallelMultiHashMap<int2, Entity>(100,Allocator.Persistent);
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId, NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        state.RequireForUpdate<ShootingConfig>();
    

        entityQueryBuilder.Dispose();


        isChanged = SystemAPI.GetComponentLookup<IsChanged>(false);
        alwaysUpdate = SystemAPI.GetComponentLookup<AlwaysUpdate>();

        getVelocity = state.GetComponentLookup<Velocity2D>();
        getPosition = state.GetComponentLookup<LocalTransform>(false);
        getHitbox = state.GetComponentLookup<BoxCollider2D>();
        getPhysics = state.GetComponentLookup<Physics2D>();
        getParent = state.GetComponentLookup<Parent>(false);
        childrenBuffer = state.GetBufferLookup<PhysicsChildrenBuffer>(true);
        getForceImpulse = state.GetComponentLookup<ForceImpulse2D>(false);
    }

}

[BurstCompile]
public partial struct UpdateEntityMap : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public MapSettings map;
    [ReadOnly] public double time;
    [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

    [BurstCompile]
    public void Execute(Entity bulletEntity,in LocalTransform localTransform,in Velocity2D velocity2D,in Bullet bullet, [EntityIndexInQuery] int sortKey)
    {
        
    }   
}