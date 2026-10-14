using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageConfig", menuName = "GameAsset/MessageConfig")]
public class MessageConfig : ScriptableObject
{   
    private static MessageConfig _instance;
    public static MessageConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<MessageConfig>("Config/MessageConfig");

            return _instance;
        }
    }
    
    [Header("Messages")]
    [Label("Message System cleanup period (seconds)")]
    [Min(0.01f)]public float CleanupPeriod;
    [Label("Message Chunk Lifetime (seconds)")]
    [Min(0.01f)]public float MessageChunkLifetime;
    
    
    [Header("RPCs")]
    [Label("RPC cleanup period (seconds)")]
    [Min(0.01f)]public float RPCCleanupPeriod;
    [Label("RPC Lifetime (ticks)")]
    [Min(1)]public uint MaxRpcAgeFrames;
}