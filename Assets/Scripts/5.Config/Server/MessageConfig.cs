using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageConfig", menuName = "GameAsset/MessageConfig")]
public class MessageConfig : Config<MessageConfig>
{   
    [Rename("Message System cleanup period "),Suffix("Seconds")]
    [Min(0.01f)]public float CleanupPeriod;
    [Rename("Message Chunk Lifetime"),Suffix("Seconds")]
    [Min(0.01f)]public float MessageChunkLifetime;
     
    [Header("RPCs")]

    [Rename("RPC cleanup period"),Suffix("Seconds")]
    [Min(0.01f)]public float RPCCleanupPeriod;
    [Rename("Max Rpc Age Frames "),Suffix("Ticks")]
    [Min(1)]public uint MaxRpcAgeFrames;
} 