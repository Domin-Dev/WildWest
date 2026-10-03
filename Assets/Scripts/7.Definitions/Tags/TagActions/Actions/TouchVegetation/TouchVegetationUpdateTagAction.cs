using System.Xml;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationUpdate", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationUpdate")]
public class TouchVegetationUpdateTagAction : TagAction<TagSettingsMaterial, TagActionArgsBase,EntityCommandBuffer,TagWithTriggerEventContext,float>
{
    public float minVelocity;
    public float startInfluenceValue;

    [Header("Durations")]
    [Min(0.01f)]public float effectDuration;
    [Min(0.01f)] public float backEffectDuration;

    private int influenceID = Shader.PropertyToID("_Influence");
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data,float deltaTime, TagSettingsMaterial tagSettings, TagActionArgsBase args)
    {
        (int index,TagActionState state)[] states = GetActionState(data.states);
        if(states.Length == 4)
        {
            int entityCounter = states[0].state.Value.Int;
            float elapsedTime = states[1].state.Value.Float;
            bool isStart = states[2].state.Value.Bool;
            float influence = states[3].state.Value.Float;

            elapsedTime += deltaTime;
            
            float lerpValue, progress;
            Entity spriteEntity = data.entityManager.GetComponentData<ClientGridObject>(data.triggerParent).sprite;
            SpriteRenderer spriteRenderer = data.entityManager.GetComponentObject<SpriteRenderer>(spriteEntity);

            if(isStart)
            {
                progress = elapsedTime/effectDuration;
                lerpValue = math.lerp(startInfluenceValue,influence,progress);
            }
            else    
            { 
                progress = elapsedTime/backEffectDuration;
                float currentInfluence = spriteRenderer.material.GetFloat(influenceID);
                lerpValue = math.lerp(currentInfluence,startInfluenceValue,progress);
            }

            spriteRenderer.material.SetFloat(influenceID,lerpValue);

            if(progress >= 1f)
            {
                ecb.RemoveComponent<TagWithTriggerEventContext>(data.trigger);
                RemoveState(states,data.states);
            }
            else
            {
                states[1].state.Value.Float = elapsedTime;
                UpdateState(states,data.states);
            }
        }
        else
        {
            ecb.RemoveComponent<TagWithTriggerEventContext>(data.trigger);
            RemoveState(states,data.states);
        }
    }
}

            
