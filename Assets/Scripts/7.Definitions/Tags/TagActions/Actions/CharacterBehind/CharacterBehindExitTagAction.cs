using System.Xml;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterBehindExit", menuName = "GameAsset/TagAction/CharacterBehind/CharacterBehindExit")]
public class CharacterBehindExitTagAction : TagAction<TagSettings, TagActionArgsBase,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public CharacterBehindSetUpTagAction setUpTag;
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettings tagSettings, TagActionArgsBase args)
    {
        var state = GetActionState(data.states,setUpTag.TagActionID);
        state[0].state.Value.Int--;

        if(state[0].state.Value.Int == 0)
        {
            Entity spriteEntity = data.entityManager.GetComponentData<ClientGridObject>(data.triggerParent).sprite;
            SpriteRenderer spriteRenderer = data.entityManager.GetComponentObject<SpriteRenderer>(spriteEntity);
            var color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
            state[1].state.Value.Bool = false;
        }
        UpdateState(state,data.states);
    }
}




