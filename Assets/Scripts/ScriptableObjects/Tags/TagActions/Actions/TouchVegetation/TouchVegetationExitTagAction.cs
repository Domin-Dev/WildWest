using System.Xml;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationExit", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationExit")]
public class TouchVegetationExitTagAction : TagAction<TagSettingsMaterial, TagActionArgsInt,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public TagActionBase updateAction;
    private int influenceID = Shader.PropertyToID("_Influence");
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettingsMaterial tagSettings, TagActionArgsInt args)
    {
        if(data.entityManager.HasComponent<TagWithTriggerEventContext>(data.trigger))
        {
            var states = GetActionState(data.states,updateAction.TagActionID);
            states[0].state.Value.Float = 0f;
            states[1].state.Value.Bool = false;
            UpdateState(states,data.states);
        }
        else
        {
            ecb.AddComponent(data.trigger,data);  
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
                Value = new TagValue(){ Bool = false} 
            });
            ecb.AppendToBuffer(data.trigger,new TagActionState()
            {
                TagActionID = updateAction.TagActionID,
                Type = TagValueType.Float,
                Value = new TagValue(){ Float = 0} 
            });
        }
    }
}




