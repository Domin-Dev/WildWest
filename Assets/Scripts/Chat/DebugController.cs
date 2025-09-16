
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
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
        commandList.Add(new DebugCommand("players", "Prints a list of players and their roles", null, (ref EntityCommandBuffer ecb,Entity e) =>
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

        //commandList.Add(new DebugCommand<int>("spawn", "Spawn", "[Number]", (x) =>
        //{
        //    Debug.Log("spawn" + x);
        //}));
        //commandList.Add(new DebugCommand<int,int>("give", "Adds item to player equipment", "[ID] [Count]", (ID, count) =>
        //{
        //   if(ItemsAsset.instance.IsItem(ID)) EquipmentManager.instance.AddNewItem(ItemsAsset.instance.GetItemStats(ID,count));
        //   else ChatManager.instance.Print("ID is't correct");
        //}));
        //commandList.Add(new DebugCommand<int>("give", "Adds item to player equipment", "[ID]", (ID) =>
        //{
        //    if (ItemsAsset.instance.IsItem(ID)) EquipmentManager.instance.AddNewItem(ItemsAsset.instance.GetItemStats(ID));
        //    else ChatManager.instance.Print("ID is't correct");
        //}));
        //commandList.Add(new DebugCommand<int>("water", "Prints information about water body", "[Water body ID]", (WaterBodyID) =>
        //{
        //    string text;
        //    LiquidsManager.instance.waterBodies.TryGetValue(WaterBodyID, out WaterBody waterBody);
        //    if(waterBody != null)
        //        text = $"Water Body ID:{WaterBodyID} TileCount:{waterBody.tileCount} Fill:{Math.Round(waterBody.fill)} FillTile:{Math.Round(waterBody.GetFillTile(),2)}";  
        //    else
        //        text = "Incorrect Water Body ID";
        //    ChatManager.instance.Print(text);
        //}));
        commandList.Add(new DebugCommand("help", "Command list", "", (ref EntityCommandBuffer entityCommandBuffer, Entity e) =>
        {
            string text = "Command list:\n";
            for (int i = 0; i < commandList.Count; i++)
            {
                CommandBase commandBase = commandList[i] as CommandBase;
                text += $"/{commandBase.commandId} {commandBase.commandFormat} - {commandBase.commandDescription}\n";
            }
            Debug.Log("dzial!! " + text);
            return text;
        },false,false));
        //commandList.Add(new DebugCommand<int,int>("tp", "Teleport oneself ", "[TileX] [TileY]", (x,y) =>
        //{
        //    string text = $"Teleport to [{x},{y}] :\n";
        //    Vector2 vec = GridVisualization.instance.GetWorldPosition(x, y);
        //  //  FindAnyObjectByType<MyCharacterController>().SetPosition(vec);
        //    ChatManager.instance.Print(text);
        //}));
        //commandList.Add(new DebugCommand<int,int,int>("wt", "Water transfer ", "[TileX] [TileY] [Number]", (x,y, water) =>
        //{
        //    GridTile gridTile = GridVisualization.instance.GetTileByGridPosition(x, y);
        //    LiquidsManager.instance.WaterTransfer(gridTile, water);
        //}));
        return commandList;
    }



}
