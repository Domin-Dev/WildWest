
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public static class DebugController 
{
    static List<CommandBase> commandList;
    public static List<CommandBase> GetCommandList()
    {
        if (commandList != null) return commandList;
        commandList = new List<CommandBase>();
        commandList.Add(new DebugCommand("host", "Start host", "", () =>
        {
            //    NetworkManager.Singleton.StartHost();
            return "";
        }));
        commandList.Add(new DebugCommand<int>("host", "Start host", "[IP adress]", (int x) =>
        {
            return "dzialak!!! " + x;
        }));
        commandList.Add(new DebugCommand<string>("mkadmin", "Start host", "[player name]", (string arg) =>
        {
            if (ClientServerBootstrap.HasServerWorld)
            {
                bool found = false;
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;

                Entity player = Entity.Null;
                var query = entityManager.CreateEntityQuery(typeof(PlayerName));
                var entities = query.ToEntityArray(Allocator.Temp);


                foreach (var entity in entities)
                {
                    var playerName = entityManager.GetComponentData<PlayerName>(entity);
                    if (!entityManager.HasComponent<Admin>(entity) && string.Compare(playerName.name.ToString(), arg) == 0)
                    {
                        player = entity;
                        found = true;
                        break;
                    }
                }



                entities.Dispose();
                query.Dispose();
                if(found)
                {
                    var ecb = new EntityCommandBuffer(Allocator.Temp);
                    ecb.AddComponent<Admin>(player);
                    RPCHelper.SendMessageToClient(ref ecb, "You are now admin!!!", player);
                    ecb.Playback(entityManager);
                    ecb.Dispose();
                }

                return found ? "The player is now admin!" : "Player not found!";
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
        commandList.Add(new DebugCommand("help", "Command list", "", () =>
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
