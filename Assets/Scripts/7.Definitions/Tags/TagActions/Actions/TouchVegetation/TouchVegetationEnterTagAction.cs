using System.Xml;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationEnter", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationEnter")]
public class TouchVegetationEnterTagAction : TagAction<TagSettingsMaterial, TagActionArgsBase,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public TouchVegetationUpdateTagAction updateAction;
    private int influenceID = Shader.PropertyToID("_Influence");

    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettingsMaterial tagSettings, TagActionArgsBase args)
    {        
        float3 currentPositon = data.entityManager.GetComponentData<LocalTransform>(data.Entity).Position;
        float3 postion = data.entityManager.GetComponentData<LocalToWorld>(data.trigger).Position;
        float influence = (currentPositon.x - postion.x > 0 ? 1 : -1) * tagSettings.influence;
        
        var state = GetActionState(data.states,updateAction.TagActionID);
        state[0].state.Value.Int++;
        UpdateState(state[0],data.states);

        if(data.entityManager.HasComponent<TagWithTriggerEventContext>(data.trigger))
        {
            if(!state[2].state.Value.Bool)
            {
                state[1].state.Value.Float = 0f;
                state[2].state.Value.Bool = true;
                state[3].state.Value.Float = influence;
            }
            UpdateState(state,data.states);
        }
        else
        {
            ecb.AppendToBuffer(data.trigger,new TagActionState()
            {
                TagActionID = updateAction.TagActionID,
                Type = TagValueType.Float,
                Value = new TagValue(){ Float = 0f},
                Temp = true
            });
            ecb.AppendToBuffer(data.trigger,new TagActionState()
            {
                TagActionID = updateAction.TagActionID,
                Type = TagValueType.Bool,
                Value = new TagValue(){ Bool = true}, 
                Temp = true
            });
            ecb.AppendToBuffer(data.trigger,new TagActionState()
            {
                TagActionID = updateAction.TagActionID,
                Type = TagValueType.Float,
                Value = new TagValue(){ Float = influence},
                Temp = true
            });         
            ecb.AddComponent(data.trigger,data);
        }
        Sounds.instance.PlayerSound(tagSettings.effectSound);
    }
}



