using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MessageServerSystem : SystemBase
{

    private List<CommandBase> commandList;
    protected override void OnCreate()
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewMessageRPC, ReceiveRpcCommandRequest>();
        RequireForUpdate(GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
        commandList = DebugController.GetCommandList();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((NewMessageRPC requestRPC, ReceiveRpcCommandRequest receiveRpc, Entity entity) in
        SystemAPI.Query<NewMessageRPC, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(entity);
            var sender = SystemAPI.GetComponent<PlayerName>(receiveRpc.SourceConnection).name;

            if (requestRPC.message.ToString().Trim()[0] != '/')
            {
                Entity message = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(message, new NewMessageServerRPC()
                {
                    message = requestRPC.message,
                    sender = sender,
                    messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    senderIsServer = false
                });
                entityCommandBuffer.AddComponent(message, new SendRpcCommandRequest());
            }
            else
            {
                CheckCommands(ref entityCommandBuffer,requestRPC.message.ToString().Trim(),receiveRpc.SourceConnection);
            }
        }
        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
    }

    public void CheckCommands(ref EntityCommandBuffer entityCommandBuffer, string command,Entity connectionEntity)
    {
        string[] properties = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0, 1);
        List<CommandBase> hints = new List<CommandBase>();

        foreach (var item in commandList)
        {
            CommandBase commandBase = item as CommandBase;
            if (string.Compare(commandBase.commandId, properties[0], true) == 0)
            {
                string[] args = properties.Skip(1).ToArray();
                if (item.Validate(args))
                {
                    if(item.isAdminCommand)
                    {
                        if(SystemAPI.HasComponent<Admin>(connectionEntity))
                        {
                            var output = item.Invoke(args, ref entityCommandBuffer, connectionEntity);
                            if (!string.IsNullOrEmpty(output))
                                RPCHelper.SendMessageToClient(ref entityCommandBuffer, output, connectionEntity);
                        }
                        else
                        {
                            RPCHelper.SendMessageToClient(ref entityCommandBuffer, "You don’t have permission!", connectionEntity);
                        }
                    }
                }      
            }
        }  
    }
}
