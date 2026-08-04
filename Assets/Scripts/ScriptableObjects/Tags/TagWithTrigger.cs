using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TagWithTrigger", menuName = "GameAsset/Tags/TagWithTrigger")]
public class TagWithTrigger : Tag<TagSettingsTrigger>
{
    public WorldType worldType;
    public LayerMask BelongsTo;
    public LayerMask CollidesWith;
    public TagEvent<EntityCommandBuffer,TagWithTriggerEventContext> onEnter;
    public TagEvent<EntityCommandBuffer,TagWithTriggerEventContext> onStay;
    public TagEvent<EntityCommandBuffer,TagWithTriggerEventContext> onExit;
    public TagEvent<EntityCommandBuffer,TagWithTriggerEventContext,float> onUpdate;

    public ReadyAction<EntityCommandBuffer,TagWithTriggerEventContext>[] GetActions(StatefulEventState eventState,Item item)
    {
        var tagEvent = GetEvent(eventState);
        return tagEvent.GetActions(item);
    }

    private TagEvent<EntityCommandBuffer,TagWithTriggerEventContext> GetEvent(StatefulEventState eventState)
    {
        switch(eventState)
        {
            case StatefulEventState.Enter:
                return onEnter;
            case StatefulEventState.Stay:
                return onStay;
            case StatefulEventState.Exit:
                return onExit;
        }
        return null;
    }
}

public struct TagWithTriggerEventContext : IComponentData
{
    public EntityManager entityManager;
    public DynamicBuffer<TagActionState> states;
    public Entity Entity;
    public Entity trigger;
    public Entity triggerParent;
}

public enum WorldType
{
    Client,
    Server,
    ClientAndServer,
}