

using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "GameAsset/ConfigFiles/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    private static PlayerConfig _instance;

    public static PlayerConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<PlayerConfig>("Config/PlayerConfig");

            return _instance;
        }
    }

    [Header("Player punch")]
    public float cooldown;
    public float hitDelay;
    public int damage;
    public Animation punchAnim;
}
