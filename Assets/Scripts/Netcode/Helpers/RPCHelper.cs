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

    public static void SendRpc<T>(ref EntityCommandBuffer ecb, in T rpcCommand)
    where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, rpcCommand);
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());
    }

    public static void SendRpc<T>(ref EntityCommandBuffer ecb)
      where T : unmanaged, IRpcCommand
    {
        Entity rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, new  T());
        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());
    }
}
