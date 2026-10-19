using UnityEngine;

[CreateAssetMenu(fileName = "WorldConfig", menuName = "GameAsset/ConfigFiles/WorldConfig")]
public class WorldConfig : Config<WorldConfig>
{
    public TagWithMaterial WindEffectTag;
    public TagWithTrigger TouchTriggerTag;
}
