using System.Xml;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationExit", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationExit")]
public class TouchVegetationExitTagAction : TagAction<TagSettingsMaterial, TagActionArgsBase,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public TagActionBase updateAction;
    private int influenceID = Shader.PropertyToID("_Influence");
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettingsMaterial tagSettings, TagActionArgsBase args)
    {
        var state = GetActionState(data.states,updateAction.TagActionID);
        state[0].state.Value.Int--;
        UpdateState(state[0],data.states);

        if(state[0].state.Value.Int <= 0)
        {
            if(data.entityManager.HasComponent<TagWithTriggerEventContext>(data.trigger))
            {
                var states = GetActionState(data.states,updateAction.TagActionID);
                states[1].state.Value.Float = 0f;
                states[2].state.Value.Bool = false;
                UpdateState(states,data.states);
            }
            else
            {
                ecb.AddComponent(data.trigger,data);  
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
                    Value = new TagValue(){ Bool = false},
                    Temp = true 
                });
                ecb.AppendToBuffer(data.trigger,new TagActionState()
                {
                    TagActionID = updateAction.TagActionID,
                    Type = TagValueType.Float,
                    Value = new TagValue(){ Float = 0}, 
                    Temp = true
                });

            }
        }
    }
}




