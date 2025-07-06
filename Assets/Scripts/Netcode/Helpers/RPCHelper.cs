using Unity.Entities;
using Unity.NetCode;

public static class RPCHelper
{
    public static void SendRpc<T>(ref EntityCommandBuffer ecb, Entity connectionEntity, in T rpcCommand)
        where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, rpcCommand);
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
    }
}
