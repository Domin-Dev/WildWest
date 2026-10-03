using System.Xml;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterBehindEnter", menuName = "GameAsset/TagAction/CharacterBehind/CharacterBehindEnter")]
public class CharacterBehindEnterTagAction : TagAction<TagSettings, TagActionArgsBase,EntityCommandBuffer,TagWithTriggerEventContext>
{
    public CharacterBehindSetUpTagAction setUpTag;
    protected override void Func(EntityCommandBuffer ecb,TagWithTriggerEventContext data, TagSettings tagSettings, TagActionArgsBase args)
    {        
        var state = GetActionState(data.states,setUpTag.TagActionID);
        state[0].state.Value.Int++;

        if(!state[1].state.Value.Bool)
        {  
            float3 currentPositon = data.entityManager.GetComponentData<LocalTransform>(data.Entity).Position;
            float3 postion = data.entityManager.GetComponentData<LocalToWorld>(data.trigger).Position;
            if(currentPositon.y > postion.y)
            { 
                Entity spriteEntity = data.entityManager.GetComponentData<ClientGridObject>(data.triggerParent).sprite;
                SpriteRenderer spriteRenderer = data.entityManager.GetComponentObject<SpriteRenderer>(spriteEntity);
                var color = spriteRenderer.color;
                color.a = setUpTag.behindAlpha;
                spriteRenderer.color = color;
                state[1].state.Value.Bool = true;
            }
        }
        UpdateState(state,data.states);
    }
}



