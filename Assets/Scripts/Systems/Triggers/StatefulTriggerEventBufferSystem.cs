using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct StatefulTriggerEventBufferSystem : ISystem
{
    private StatefulSimulationEventBuffers<StatefulTriggerEvent> stateFulEventBuffers;
    private ComponentHandles componentHandles;
    private EntityQuery triggerEventQuery;
    struct ComponentHandles
    {
        public ComponentLookup<StatefulTriggerEventExclude> EventExcludes;
        public BufferLookup<StatefulTriggerEvent> EventBuffers;
        public ComponentLookup<HasEvents> HasEventsLookup;

        public ComponentHandles(ref SystemState systemState)
        {
            EventExcludes = systemState.GetComponentLookup<StatefulTriggerEventExclude>(true);
            EventBuffers = systemState.GetBufferLookup<StatefulTriggerEvent>();
            HasEventsLookup = systemState.GetComponentLookup<HasEvents>();
        }

        public void Update(ref SystemState systemState)
        {
            EventExcludes.Update(ref systemState);
            EventBuffers.Update(ref systemState);
            HasEventsLookup.Update(ref systemState);
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAllRW<StatefulTriggerEvent>()
            .WithNone<StatefulTriggerEventExclude>();

        stateFulEventBuffers = new StatefulSimulationEventBuffers<StatefulTriggerEvent>();
        stateFulEventBuffers.AllocateBuffers();

        triggerEventQuery = state.GetEntityQuery(builder);
        state.RequireForUpdate(triggerEventQuery);

        componentHandles = new ComponentHandles(ref state);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        stateFulEventBuffers.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if(!networkTime.IsFirstTimeFullyPredictingTick) return;
        
        componentHandles.Update(ref state);
      //  state.Dependency = new ClearTriggerEventDynamicBufferJob()
       //     .ScheduleParallel(triggerEventQuery, state.Dependency);


        stateFulEventBuffers.SwapBuffers();

        var currentEvents = stateFulEventBuffers.Current;
        var previousEvents = stateFulEventBuffers.Previous;

        state.Dependency = new CollectTriggerEvents
        {
            TriggerEvents = currentEvents
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);


        state.Dependency = new ConvertEventStreamToDynamicBufferJob<StatefulTriggerEvent, StatefulTriggerEventExclude>
        {
            CurrentEvents = currentEvents,
            PreviousEvents = previousEvents,
            EventLookup = componentHandles.EventBuffers,
            HasEventLookup = componentHandles.HasEventsLookup, 

            UseExcludeComponent = false,
            EventExcludeLookup = componentHandles.EventExcludes
        }.Schedule(state.Dependency);
    }
    
    [BurstCompile]
    public partial struct ClearTriggerEventDynamicBufferJob : IJobEntity
    {
        public void Execute(ref DynamicBuffer<StatefulTriggerEvent> eventBuffer) => eventBuffer.Clear();
    }

    [BurstCompile]
    public struct CollectTriggerEvents : ITriggerEventsJob
    {
        public NativeList<StatefulTriggerEvent> TriggerEvents;
        public void Execute(TriggerEvent triggerEvent) => TriggerEvents.Add(new StatefulTriggerEvent(triggerEvent));
    }

    [BurstCompile]
    public struct ConvertEventStreamToDynamicBufferJob<T, C> : IJob
        where T : unmanaged, IBufferElementData, IStatefulSimulationEvent<T>
        where C : unmanaged, IComponentData
    {
        public NativeList<T> PreviousEvents;
        public NativeList<T> CurrentEvents;
        public BufferLookup<T> EventLookup;
        public ComponentLookup<HasEvents> HasEventLookup;

        public bool UseExcludeComponent;
        [ReadOnly] public ComponentLookup<C> EventExcludeLookup;


        public void Execute()
        {
            var statefulEvents = new NativeList<T>(CurrentEvents.Length, Allocator.Temp);
            StatefulSimulationEventBuffers<T>.GetStatefulEvents(PreviousEvents, CurrentEvents, statefulEvents);

            for (int i = 0; i < statefulEvents.Length; i++)
            {
                var statefulEvent = statefulEvents[i];

                var addToEntityA = EventLookup.HasBuffer(statefulEvent.EntityA) &&
                    (!UseExcludeComponent || !EventExcludeLookup.HasComponent(statefulEvent.EntityA));
                var addToEntityB = EventLookup.HasBuffer(statefulEvent.EntityB) &&
                    (!UseExcludeComponent || !EventExcludeLookup.HasComponent(statefulEvent.EntityB));

                if (addToEntityA)
                {
                    EventLookup[statefulEvent.EntityA].Add(statefulEvent);
                    HasEventLookup.SetComponentEnabled(statefulEvent.EntityA,true);
                }

                if (addToEntityB)
                {
                    EventLookup[statefulEvent.EntityB].Add(statefulEvent);
                    HasEventLookup.SetComponentEnabled(statefulEvent.EntityB,true);
                }
            }
        }
    }


}