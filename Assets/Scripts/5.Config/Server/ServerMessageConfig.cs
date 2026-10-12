using UnityEngine;

[CreateAssetMenu(fileName = "ServerMessageConfig", menuName = "GameAsset/ServerMessageConfig")]
public class ServerMessageConfig : ScriptableObject
{   
    private static ServerMessageConfig _instance;
    public static ServerMessageConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ServerMessageConfig>("Config/ServerMessageConfig");

            return _instance;
        }
    }

    [NaughtyAttributes.Label("Server Message System cleanup period (seconds)")]
    [Min(0.01f)]public float CleanupPeriod;
    [NaughtyAttributes.Label("Message Chunk Lifetime (seconds)")]
    [Min(0.01f)]public float MessageChunkLifetime;
}