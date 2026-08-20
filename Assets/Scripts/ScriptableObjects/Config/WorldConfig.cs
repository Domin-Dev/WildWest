

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


    public static WorldTimeConfig TimeConfig => Instance.worldTimeConfig;
    public static WeatherConfig WeatherConfig => Instance.weatherConfig;
    public static UIConfig UIConfig => Instance.uIConfig;
    public static SoundsConfig SoundsConfig => Instance.soundsConfig;

    [SerializeField] private WorldTimeConfig worldTimeConfig;
    [SerializeField] private WeatherConfig weatherConfig;
    [SerializeField] private UIConfig uIConfig;
    [SerializeField] private SoundsConfig soundsConfig;
    [Space]

    public TagWithMaterial WindEffectTag;
    public TagWithTrigger TouchTriggerTag;
}
