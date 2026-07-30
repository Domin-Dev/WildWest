

using UnityEngine;

[CreateAssetMenu(fileName = "WorldConfig", menuName = "GameAsset/ConfigFiles/WorldConfig")]
public class WorldConfig : ScriptableObject
{
    private static WorldConfig _instance;

    public static WorldConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<WorldConfig>("Config/WorldConfig");

            return _instance;
        }
    }
    public TagWithMaterial windEffectTag;
}
