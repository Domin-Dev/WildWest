
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class DebugController 
{
    static List<CommandBase> commandList;
    public static List<CommandBase> GetCommandList()
    {
        if (commandList != null) return commandList;
        commandList = new List<CommandBase>();
        commandList.Add(new DebugCommand<string>("mkadmin", "Grants admin privileges to a player.", "[player name]", (ref EntityCommandBuffer ecb,Entity e, string arg) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                bool found = false;
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(PlayerName));
                var entities = query.ToEntityArray(Allocator.Temp);

                foreach (var entity in entities)
                {
                    var playerName = entityManager.GetComponentData<PlayerName>(entity);
                    if (!entityManager.HasComponent<Admin>(entity) && string.Compare(playerName.name.ToString(), arg) == 0)
                    {
                        ecb.AddComponent<Admin>(entity);
                        RPCHelper.SendMessageToClient(ref ecb, "You are now admin!!!", entity);
                        found = true;
                        break;
                    }
                }
                entities.Dispose();
                query.Dispose();

                return found ? "The player is now admin!" : "Player not found!";
            }
            return null;
        }));
        commandList.Add(new DebugCommand<int,int>("tp", "Teleports to the selected position", "[Position x] [Position y]", (ref EntityCommandBuffer ecb,Entity e, int x, int y) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(PlayerName));
                var entities = query.ToEntityArray(Allocator.Temp);

                foreach (var entity in entities)
                {
                    if(entity == e)
                    {
                        var character = entityManager.GetComponentData<LinkedCharacter>(e).entity;
                        var position =  LocalTransform.FromPosition(MapServerSystem.Map.MapPositionToWorldPosition(x, y));
                        ecb.SetComponent(character, position);
                        ecb.SetComponentEnabled<IsChanged>(character, true);
                        CollisionSystem.PlayerChangeChunk(entityManager, ref ecb, position, character);
                    }
                }
                entities.Dispose();
                query.Dispose();
            }
            return null;
        }));
        commandList.Add(new DebugCommand("players", "Prints a list of players and their roles", null, (ref EntityCommandBuffer ecb, Entity e) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(PlayerName));
                var entities = query.ToEntityArray(Allocator.Temp);
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine();
                stringBuilder.Append(("Name").PadRight(32));
                stringBuilder.Append("Role");
                stringBuilder.AppendLine();

                foreach (var entity in entities)
                {
                    var playerName = entityManager.GetComponentData<PlayerName>(entity);
                    stringBuilder.Append(playerName.name.ToString().PadRight(32));
                    stringBuilder.Append(entityManager.HasComponent<Admin>(entity) ? "Administrator" : "Player");
                    stringBuilder.AppendLine();
                }
                entities.Dispose();
                query.Dispose();
                return stringBuilder.ToString();
            }
            return null;
        }));

        #region Give
        commandList.Add(new DebugCommand<int,int,string>("give", "Gives the player the specified item", "[Item ID] [Quantity] [player name]", (ref EntityCommandBuffer ecb, Entity e,int id,int quantity,string player) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                if (ItemsAsset.instance.GetItem(id) == null) return null;
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(PlayerName),typeof(NetworkId));
                var names = query.ToComponentDataArray<PlayerName>(Allocator.TempJob);
                var players = query.ToEntityArray(Allocator.TempJob);
                Entity networkPlayer = Entity.Null;
                
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i].name == player)
                    {
                        networkPlayer = players[i];
                        break;
                    }
                }
                players.Dispose();
                names.Dispose();
                query.Dispose();
                if (networkPlayer == Entity.Null)
                    return "No player found with that name";
                else
                {
                    EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
                    {
                        itemID = id,
                        quantity = quantity,
                        networkEntity = networkPlayer
                    });
                }
            }
            return null;
        }));
        commandList.Add(new DebugCommand<int, int>("give", "Gives you the specified item", "[Item ID] [Quantity] ", (ref EntityCommandBuffer ecb, Entity e, int id, int quantity) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                if (ItemsAsset.instance.GetItem(id) == null) return null;

                EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
                {
                    itemID = id,
                    quantity = quantity,
                    networkEntity = e
                });
            }
            return null;
        }));
        commandList.Add(new DebugCommand<int>("give", "Gives you the specified item", "[Item ID]", (ref EntityCommandBuffer ecb, Entity e, int id) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                if (ItemsAsset.instance.GetItem(id) == null) return null;

                EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
                {
                    itemID = id,
                    quantity = 1,
                    networkEntity = e
                });
            }
            return null;
        }));
        #endregion

        commandList.Add(new DebugCommand("cleareq", "Removes all items from your inventory", "", (ref EntityCommandBuffer ecb, Entity e) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                EntityHelper.CreateEntityWithComponent(ref ecb, new EQClear()
                {
                    containerIndex = -1,
                    networkEntity = e
                });
            }
            return null;
        }));
        commandList.Add(new DebugCommand<int>("cleareq", "Removes all items from your inventory", "[Container Index]", (ref EntityCommandBuffer ecb, Entity e,int index) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                EntityHelper.CreateEntityWithComponent(ref ecb, new EQClear()
                {
                    containerIndex = index,
                    networkEntity = e
                });
            }
            return null;
        }));



        commandList.Add(new DebugCommand("help", "Command list", "", (ref EntityCommandBuffer entityCommandBuffer, Entity e) =>
        {
            string text = "Command list:\n";
            for (int i = 0; i < commandList.Count; i++)
            {
                CommandBase commandBase = commandList[i] as CommandBase;
                text += $"/<Color=#{ChatManager.instance.highlightColor.ToHexString()}>{commandBase.commandId}</Color> {commandBase.commandFormat} - {commandBase.commandDescription}\n";
            }
            Debug.Log("dzial!! " + text);
            return text;
        },false,false));
        return commandList;
    }



}
