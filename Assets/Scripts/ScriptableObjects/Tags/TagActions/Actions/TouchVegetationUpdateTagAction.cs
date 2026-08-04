using System.Xml;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationUpdate", menuName = "GameAsset/TagAction/TouchVegetationUpdateTagAction")]
public class TouchVegetationUpdateTagAction : TagAction<TagSettingsMaterial, TagActionArgsInt,EntityCommandBuffer,TagWithTriggerEventContext,float>
{
    [Min(0.01f)]public float effectDuration;
    [Min(0.01f)] public float backEffectDuration;
    public float startInfluenceValue;
    private int influenceID = Shader.PropertyToID("_Influence");
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data,float deltaTime, TagSettingsMaterial tagSettings, TagActionArgsInt args)
    {
        (int index,TagActionState state)[] states = GetActionState(data.states);
       // Debug.Log(" argumenty " +  states.Length);
        if(states.Length == 2)
        {
            float elapsedTime = states[0].state.Value.Float;
            bool isStart = states[1].state.Value.Bool;

            elapsedTime += deltaTime;
            
            float lerpValue, progress;
            Entity spriteEntity = data.entityManager.GetComponentData<ClientGridObject>(data.triggerParent).sprite;
            SpriteRenderer spriteRenderer = data.entityManager.GetComponentObject<SpriteRenderer>(spriteEntity);

            if(isStart)
            {
                progress = elapsedTime/effectDuration;
                lerpValue = math.lerp(startInfluenceValue,0.6f,progress);
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

            
