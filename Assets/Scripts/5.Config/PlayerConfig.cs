using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "GameAsset/ConfigFiles/PlayerConfig")]
public class PlayerConfig : Config<PlayerConfig>
{
    [Header("Player punch")]
    [Suffix("Seconds")]
    public float cooldown;
    [Suffix("Seconds")]
    public float hitDelay;
    public int damage;
    public Animation punchAnim;
}
