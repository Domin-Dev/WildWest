using System.Xml;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationEnter", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationEnter")]
public class TouchVegetationEnterTagAction : TagAction<TagSettingsMaterial, TagActionArgsInt,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public TouchVegetationUpdateTagAction updateAction;
    private int influenceID = Shader.PropertyToID("_Influence");

    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettingsMaterial tagSettings, TagActionArgsInt args)
    {        
        float3 velocity = data.entityManager.GetComponentData<PhysicsGraphicalInterpolationBuffer>(data.Entity).PreviousVelocity.Linear;
        float length = math.length(velocity) + 1f;
        float influence = (velocity.x < 0 ? 1 : -1) * length * updateAction.influenceStrength + 1f;
        
        if(length > 0)
        {
            if(data.entityManager.HasComponent<TagWithTriggerEventContext>(data.trigger))
            {
                var state = GetActionState(data.states,updateAction.TagActionID);
                state[0].state.Value.Float = 0f;
                state[1].state.Value.Bool = true;
                state[2].state.Value.Float = influence;
                UpdateState(state,data.states);
            }
            else
            {
                ecb.AppendToBuffer(data.trigger,new TagActionState()
                {
                    TagActionID = updateAction.TagActionID,
                    Type = TagValueType.Float,
                    Value = new TagValue(){ Float = 0f} 
                });
                ecb.AppendToBuffer(data.trigger,new TagActionState()
                {
                    TagActionID = updateAction.TagActionID,
                    Type = TagValueType.Bool,
                    Value = new TagValue(){ Bool = true} 
                });
                ecb.AppendToBuffer(data.trigger,new TagActionState()
                {
                    TagActionID = updateAction.TagActionID,
                    Type = TagValueType.Float,
                    Value = new TagValue(){ Float = influence} 
                });
                ecb.AddComponent(data.trigger,data);
            }
            Sounds.instance.PlayerSound(tagSettings.effectSound);
        }
    }
}



