using System.Xml;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "TouchVegetationSetUp", menuName = "GameAsset/TagAction/TouchVegetation/TouchVegetationSetUp")]
public class TouchVegetationSetUpTagAction : TagAction<TagSettingsMaterial, TagActionArgsInt,EntityCommandBuffer,Entity>
{
    public TouchVegetationUpdateTagAction updateAction;
    protected override void Func(EntityCommandBuffer ecb,Entity trigger, TagSettingsMaterial tagSettings, TagActionArgsInt args)
    {
        ecb.AppendToBuffer(trigger,new TagActionState()
        {
            TagActionID = updateAction.TagActionID,
            Type = TagValueType.Int,
            Value = new TagValue(){ Int = 0},
            Temp = false
        });
    }
}

            
