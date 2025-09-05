// The approval RPC, here it contains a hypothetical payload the server will validate
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

public struct ApprovalFlow : IApprovalRpcCommand
{
    public FixedString512Bytes Payload;
}

// This is used to indicate we've already sent an approval RPC and don't need to do so again
public struct ApprovalStarted : IComponentData
{
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct ClientConnectionApprovalSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RpcCollection>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (connection, entity) in SystemAPI.Query<RefRW<NetworkStreamConnection>>().WithNone<NetworkId>().WithNone<ApprovalStarted>().WithEntityAccess())
        {
            if (connection.ValueRW.CurrentState == ConnectionState.State.Approval)
            {
                var sendApprovalMsg = ecb.CreateEntity();
                ecb.AddComponent(sendApprovalMsg, new ApprovalFlow { Payload = "ABC" });
                ecb.AddComponent<SendRpcCommandRequest>(sendApprovalMsg);
                
                ecb.AddComponent<ApprovalStarted>(entity);
            }
        }
        ecb.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerConnectionApprovalSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (receiveRpc, approvalMsg, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRW<ApprovalFlow>>().WithEntityAccess())
        {
            var connectionEntity = receiveRpc.ValueRO.SourceConnection;
            var connection = state.EntityManager.GetComponentData<NetworkStreamConnection>(connectionEntity);
            var driver = SystemAPI.GetSingletonRW<NetworkStreamDriver>().ValueRO;
            var remoteEP = driver.GetRemoteEndPoint(connection);
            string ip = remoteEP.Address.ToString();
            ushort port = remoteEP.Port;


            Debug.Log($"Klient połączony z IP: {ip}, port: {port}");
            if (approvalMsg.ValueRO.Payload.Equals("ABC"))
            {
                ecb.AddComponent<ConnectionApproved>(connectionEntity);
                ecb.DestroyEntity(entity);
            }
            else
            {
                ecb.AddComponent(connectionEntity,new NetworkStreamRequestDisconnect() { Reason = NetworkStreamDisconnectReason.AuthenticationFailure});
            }
        }
        ecb.Playback(state.EntityManager);
    }
}
