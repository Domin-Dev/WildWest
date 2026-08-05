using System.Xml;
using Unity.Entities;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationStay", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationStay")]
public class TouchVegetationStayTagAction : TagAction<TagSettingsMaterial, TagActionArgsInt,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public TagActionBase updateAction;
    private int influenceID = Shader.PropertyToID("_Influence");
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettingsMaterial tagSettings, TagActionArgsInt args)
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
            //Value = new TagValue(){ Bool = true} 
        });
        ecb.AddComponent(data.trigger,data);
        Sounds.instance.PlayerSound(tagSettings.effectSound);
    }
}



