using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct MessageServerSystem : ISystem
{

    private List<object> commandList;

    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewMessageRPC,ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
        commandList = DebugController.GetCommandList();
    }

    public void OnUpdate(ref SystemState state)
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
               CheckCommands(requestRPC.message.ToString().Trim());
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }


    public void CheckCommands(string command)
    {
        string[] properties = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0, 1);
        List<CommandBase> hints = new List<CommandBase>();

        foreach (var item in commandList)
        {
            CommandBase commandBase = item as CommandBase;
            if (string.Compare(commandBase.commandId, properties[0], true) == 0)
            {
                if (item is DebugCommand)
                {
                    (item as DebugCommand).Invoke();
                    return;
                }
                else if (item is DebugCommand<int>)
                {
                    int arg;
                    if (properties.Length > 1 && int.TryParse(properties[1], out arg))
                    {
                        (item as DebugCommand<int>).Invoke(arg);
                        return;
                    }
                    else hints.Add(commandBase);
                }
                else if (item is DebugCommand<int, int>)
                {
                    int arg1, arg2;
                    if (properties.Length > 2 && int.TryParse(properties[1], out arg1) && int.TryParse(properties[2], out arg2))
                    {
                        (item as DebugCommand<int, int>).Invoke(arg1, arg2);
                        return;
                    }
                    else hints.Add(commandBase);
                }
                else if (item is DebugCommand<int, int, int>)
                {
                    int arg1, arg2, arg3;
                    if (properties.Length > 2 && int.TryParse(properties[1], out arg1) && int.TryParse(properties[2], out arg2) && int.TryParse(properties[3], out arg3))
                    {
                        (item as DebugCommand<int, int, int>).Invoke(arg1, arg2, arg3);
                        return;
                    }
                    else hints.Add(commandBase);
                }
            }
        }

        //if (hints.Count == 0)
        //{
        //    Print("<Color=red>Incorrect command: </color>" + command);
        //}
        //PrintHint(hints.ToArray());
    }
}
