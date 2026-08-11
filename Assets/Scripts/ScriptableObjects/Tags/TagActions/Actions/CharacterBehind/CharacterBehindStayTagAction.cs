using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterBehindStay", menuName = "GameAsset/TagAction/CharacterBehind/CharacterBehindStay")]
public class CharacterBehindStayTagAction : TagAction<TagSettings, TagActionArgsInt,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public CharacterBehindSetUpTagAction setUpTag;
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettings tagSettings, TagActionArgsInt args)
    {
        var state = GetActionState(data.states,setUpTag.TagActionID);
        float3 currentPositon = data.entityManager.GetComponentData<LocalTransform>(data.Entity).Position;
        float3 postion = data.entityManager.GetComponentData<LocalToWorld>(data.trigger).Position;
        bool behind = currentPositon.y > postion.y;

        if(state[1].state.Value.Bool && !behind)
        {
            Entity spriteEntity = data.entityManager.GetComponentData<ClientGridObject>(data.triggerParent).sprite;
            SpriteRenderer spriteRenderer = data.entityManager.GetComponentObject<SpriteRenderer>(spriteEntity);
            var color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
            state[1].state.Value.Bool = false;
            UpdateState(state,data.states);
        }
        else if(!state[1].state.Value.Bool && behind)
        {
            Entity spriteEntity = data.entityManager.GetComponentData<ClientGridObject>(data.triggerParent).sprite;
            SpriteRenderer spriteRenderer = data.entityManager.GetComponentObject<SpriteRenderer>(spriteEntity);
            var color = spriteRenderer.color;
            color.a = setUpTag.behindAlpha;
            spriteRenderer.color = color;
            state[1].state.Value.Bool = true;
            UpdateState(state,data.states);
        }
    }


}




