using System.Xml;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationUpdate", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationUpdate")]
public class TouchVegetationUpdateTagAction : TagAction<TagSettingsMaterial, TagActionArgsInt,EntityCommandBuffer,TagWithTriggerEventContext,float>
{

    public float influenceStrength;
    public float minVelocity;
    public float startInfluenceValue;

    [Header("Durations")]
    [Min(0.01f)]public float effectDuration;
    [Min(0.01f)] public float backEffectDuration;

    private int influenceID = Shader.PropertyToID("_Influence");
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data,float deltaTime, TagSettingsMaterial tagSettings, TagActionArgsInt args)
    {
        (int index,TagActionState state)[] states = GetActionState(data.states);
        if(states.Length == 3)
        {
            float elapsedTime = states[0].state.Value.Float;
            bool isStart = states[1].state.Value.Bool;
            float influence = states[2].state.Value.Float;

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

           //Debug.Log(lerpValue + " " + deltaTime + " " + elapsedTime + " " + progress);
            spriteRenderer.material.SetFloat(influenceID,lerpValue);

            if(progress >= 1f)
            {
                ecb.RemoveComponent<TagWithTriggerEventContext>(data.trigger);
                RemoveState(states,data.states);
            }
            else
            {
                states[0].state.Value.Float = elapsedTime;
                UpdateState(states,data.states);
            }
          //  Debug.Log("dziala git !!");
        }
        else
        {
           //Debug.Log("brak argumentow");
            ecb.RemoveComponent<TagWithTriggerEventContext>(data.trigger);
            RemoveState(states,data.states);
        }
    }
}

            
