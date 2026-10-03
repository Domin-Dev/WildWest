using System.Xml;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterBehindSetUp", menuName = "GameAsset/TagAction/CharacterBehind/CharacterBehindSetUp")]
public class CharacterBehindSetUpTagAction : TagAction<TagSettings, TagActionArgsBase,EntityCommandBuffer,Entity>
{
    [Range(0,1)]
    public float behindAlpha;
    protected override void Func(EntityCommandBuffer ecb,Entity trigger, TagSettings tagSettings, TagActionArgsBase args)
    {
        ecb.AppendToBuffer(trigger,new TagActionState()
        {
            TagActionID = TagActionID,
            Type = TagValueType.Int,
            Value = new TagValue(){ Int = 0},
            Temp = false
        });        
        ecb.AppendToBuffer(trigger,new TagActionState()
        {
            TagActionID = TagActionID,
            Type = TagValueType.Bool,
            Value = new TagValue(){ Bool = false},
            Temp = false
        });
    }
}


            
