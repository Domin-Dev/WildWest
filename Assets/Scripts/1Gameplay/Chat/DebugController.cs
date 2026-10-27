using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;
public static class DebugController 
{
    static Dictionary<string,CommandBase> commands;
    public static Dictionary<string,CommandBase> Commands
    {
        get
        {
            if (commands != null) return commands;
            commands = new Dictionary<string,CommandBase>();
            Add(new DebugCommand<string>("mkadmin", "Grants admin privileges to a player.", "player name", (ref EntityCommandBuffer ecb,Entity e, string arg) =>
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
                            ChatService.ServerSendsMessage(ecb, "You are now admin!!!", entity);
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
            Add(new DebugCommand<int,int>("tp", "Teleports to the selected position", "Position x","Position y", (ref EntityCommandBuffer ecb,Entity e, int x, int y) =>
            {
                if (ClientServerBootstrap.HasServerWorld)
                {
                    var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                    var query = entityManager.CreateEntityQuery(typeof(PlayerName));
                    var entities = query.ToEntityArray(Allocator.Temp);
                    var mapQuery = entityManager.CreateEntityQuery(typeof(MapSettings));
                    MapSettings map = mapQuery.GetSingleton<MapSettings>();
                    foreach (var entity in entities)
                    {
                        if(entity == e)
                        {
                            var character = entityManager.GetComponentData<LinkedCharacter>(e).entity;
                            GhostChunk ghostChunk = entityManager.GetComponentData<GhostChunk>(character);
                            var position =  LocalTransform.FromPosition(map.GetTilePositionToEnginePosition(x, y));
                            ecb.SetComponent(character, position);
                            ghostChunk.SetNewChunk();
                            ecb.SetComponent(character,ghostChunk);
                       // CollisionSystem.GhostChangeChunk(entityManager, ref ecb, position, character);
                        }
                    }
                    mapQuery.Dispose();
                    entities.Dispose();
                    query.Dispose();
                }
                return null;
            }));
            Add(new DebugCommand("players", "Prints a list of players and their roles", (ref EntityCommandBuffer ecb, Entity e) =>
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
            #region Map
            Add(new DebugCommand("seed", "Displays world seed", (ref EntityCommandBuffer ecb, Entity e) =>
            {
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(MapSettings));
                var entities = query.ToEntityArray(Allocator.Temp);
                string result = null;
                if(entities.Length > 0)
                {
                    MapSettings map = entityManager.GetComponentData<MapSettings>(entities[0]);
                    result = $"Seed : {map.seed}";
                }
                entities.Dispose();
                query.Dispose();
                return result;
            },commandType:CommandType.ClientSide));
            #endregion
            #region Give
            // Add(new DebugCommand<int,int,string>("give", "Gives the player the specified item", "Item ID","Quantity","player name", (ref EntityCommandBuffer ecb, Entity e,int id,int quantity,string player) =>
            // {
            //     if (ClientServerBootstrap.HasServerWorld)
            //     {
            //         if (ItemsAsset.instance.GetItem(id) == null) return null;
            //         var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
            //         var query = entityManager.CreateEntityQuery(typeof(PlayerName),typeof(NetworkId));
            //         var names = query.ToComponentDataArray<PlayerName>(Allocator.TempJob);
            //         var players = query.ToEntityArray(Allocator.TempJob);
            //         Entity networkPlayer = Entity.Null;
            //         for (int i = 0; i < names.Length; i++)
            //         {
            //             if (names[i].name == player)
            //             {
            //                 networkPlayer = players[i];
            //                 break;
            //             }
            //         }
            //         players.Dispose();
            //         names.Dispose();
            //         query.Dispose();
            //         if (networkPlayer == Entity.Null)
            //             return "No player found with that name";
            //         else
            //         {
            //             EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
            //             {
            //                 item = new InventorySlot
            //                 {
            //                     itemId = id,
            //                     quantity = quantity,
            //                 },
            //                 barValue = 1,
            //                 networkEntity = networkPlayer
            //             });
            //         }
            //     }
            //     return null;
            // }));
            // Add(new DebugCommand<int, int>("give", "Gives you the specified item", "Item ID","Quantity", (ref EntityCommandBuffer ecb, Entity e, int id, int quantity) =>
            // {
            //     if (ClientServerBootstrap.HasServerWorld)
            //     {
            //         if (ItemsAsset.instance.GetItem(id) == null) return null;
            //         EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
            //         {
            //             item = new InventorySlot
            //             {
            //                 itemId = id,
            //                 quantity = quantity
            //             },
            //             barValue = 1,
            //             networkEntity = e
            //         });
            //     }
            //     return null;
            // }));            
            Add(new DebugCommand<int, int,float,int,float,MyColor>("give", "Gives you the specified item", "Item ID", "Quantity", "Wetness (0 - 100)", "Quality (0 - 6)", "Bar Value", "Color", (ref EntityCommandBuffer ecb, Entity e, int id, int quantity,float wetness,int quality,float barValue,MyColor color) =>
            {
                if (ClientServerBootstrap.HasServerWorld)
                {
                    if (ItemsAsset.instance.GetItem(id) == null) return null;
                    EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
                    {
                        item = new InventorySlot
                        {
                            itemId = id,
                            wetness = wetness,
                            quality = (Quality)quality,
                            quantity = quantity,
                            color = color,
                        },
                        barValue = barValue,
                        networkEntity = e
                    });
                }
                return null;
            }));
            // Add(new DebugCommand<int>("give", "Gives you the specified item", "Item ID", (ref EntityCommandBuffer ecb, Entity e, int id) =>
            // {
            //     if (ClientServerBootstrap.HasServerWorld)
            //     {
            //         if (ItemsAsset.instance.GetItem(id) == null) return null;
            //         EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
            //         {
            //             item = new InventorySlot
            //             {
            //                 itemId = id,
            //                 quantity = 1
            //             },
            //             barValue = 1,
            //             networkEntity = e
            //         });
            //     }
            //     return null;
            // }));
            Add(new DebugCommand("giveoutfit", "debug outfit kit", (ref EntityCommandBuffer ecb, Entity e) =>
            {
                if (ClientServerBootstrap.HasServerWorld)
                {
                    int[] items = {11,12,133,14,15,16,17,18,47};
                    foreach (int i in items)
                    {
                        EntityHelper.CreateEntityWithComponent(ref ecb, new EQGiveItem()
                        {
                            item = new InventorySlot
                            {
                                itemId = i,
                                quantity = 1
                            },
                            barValue = 1,
                            networkEntity = e
                        });
                    }
                }
                return null;
            }));
            #endregion
            #region Weather
            Add(new DebugCommand<float,float>("rain", "", "intensity", "time (s)", (ref EntityCommandBuffer ecb, Entity e,float intensity,float time) =>
            {
                if (ClientServerBootstrap.HasServerWorld)
                {
                    EntityHelper.CreateEntityWithComponent(ref ecb, new Rain()
                    {
                        intensity = intensity,
                        time = time,
                        tick = NetworkTick.Invalid
                    });
                }
                return null;
            }));       
            Add(new DebugCommand<int,int>("weatherprediction", "", "offset", "count", (ref EntityCommandBuffer ecb, Entity e,int offset,int count) =>
            {
                if(count < 0) return StringsHelper.GetColorfulString("The value must be greater than 0.",Color.red);         
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(CurrentTime));
                var entities = query.ToEntityArray(Allocator.Temp);    
                var query2 = entityManager.CreateEntityQuery(typeof(MapSettings));
                var entities2 = query2.ToEntityArray(Allocator.Temp);
                StringBuilder stringBuilder = new StringBuilder();
                if(entities.Length > 0 && entities2.Length > 0)
                {
                    CurrentTime currentTime = entityManager.GetComponentData<CurrentTime>(entities[0]);
                    MapSettings mapSettings = entityManager.GetComponentData<MapSettings>(entities2[0]);
                    var character = entityManager.GetComponentData<LinkedCharacter>(e).entity;
                    var position = entityManager.GetComponentData<LocalToWorld>(character).Position;
                    var tab = WeatherService.WeatherPrediction(mapSettings.seed,currentTime.WorldTime,position.XY(),offset,count);
                    foreach(var i in tab)
                    {
                        var time = TimeService.GetTime(i.worldTime);
                        stringBuilder.AppendLine($"Time : {time.hour}:{time.minute.ToString("00")}, Day : {time.day}".GetColorfulString(ChatConfig.Instance.HighlightColor));
                        stringBuilder.Append($"\n{i.localWeather.ToString()}");
                    }
                }
                entities.Dispose();
                query.Dispose();
                return stringBuilder.ToString();
            }));

            #endregion
            #region Time
            Add(new DebugCommand("time", "Displays information about the time", (ref EntityCommandBuffer ecb, Entity e) =>
            {
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(CurrentTime));
                var entities = query.ToEntityArray(Allocator.Temp);
                string result = null;
                if(entities.Length > 0)
                {
                    CurrentTime currentTime = entityManager.GetComponentData<CurrentTime>(entities[0]);
                    result = $"Time : {currentTime.HourInt}:{currentTime.MinuteInt.ToString("00")}\nDay : {currentTime.Day}\nWorldTime : {currentTime.WorldTime.ToString("F2")}";
                }
                entities.Dispose();
                query.Dispose();
                return result;
            },commandType: CommandType.ClientSide));
            Add(new DebugCommand<int>("timeset", "Set the specified time", "Hour", (ref EntityCommandBuffer ecb, Entity e,int value) =>
            {
                if(value < 0 || value > 24) return "The value must be between 0 and 24.";         
                if(value == 24) value = 0;
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(CurrentTime));
                var entities = query.ToEntityArray(Allocator.Temp);
                if(entities.Length > 0)
                {
                    CurrentTime currentTime = entityManager.GetComponentData<CurrentTime>(entities[0]);
                    float currentHour = currentTime.Hour;
                    double newWorldTime;
                    if(currentHour > value)
                        newWorldTime = TimeService.GetWorldTime(value,currentTime.Day + 1);
                    else 
                        newWorldTime = currentTime.GetWorldTime(value);
                    EntityHelper.CreateEntityWithComponent(ecb,new SetTimeRequest(){ newWorldtime = newWorldTime});
                }
                entities.Dispose();
                query.Dispose();
                return null;
            }));
            Add(new DebugCommand<int>("timeskip", "Skip the specific time period", "Hour", (ref EntityCommandBuffer ecb, Entity e,int value) =>
            {
                if(value < 0) return StringsHelper.GetColorfulString("The value must be greater than 0.",Color.red);         
                var entityManager = ClientServerBootstrap.ServerWorld.EntityManager;
                var query = entityManager.CreateEntityQuery(typeof(CurrentTime));
                var entities = query.ToEntityArray(Allocator.Temp);
                if(entities.Length > 0)
                {
                    CurrentTime currentTime = entityManager.GetComponentData<CurrentTime>(entities[0]);
                    EntityHelper.CreateEntityWithComponent(ecb,new SetTimeRequest(){ newWorldtime = currentTime.WorldTime + value});
                }
                entities.Dispose();
                query.Dispose();
                return null;
            }));
            #endregion
            Add(new DebugCommand("cleareq", "Removes all items from your inventory", (ref EntityCommandBuffer ecb, Entity e) =>
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
            Add(new DebugCommand<int>("cleareq", "Removes all items from your inventory", "Container Index", (ref EntityCommandBuffer ecb, Entity e,int index) =>
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
            #region Chat
            Add(new DebugCommand("clear", "Clears the chat", (ref EntityCommandBuffer entityCommandBuffer, Entity e) =>
            {
                Chat.Instance.Clear();
                return null;
            },CommandType.ClientSide));
            Add(new DebugCommand("help", "Command list",(ref EntityCommandBuffer entityCommandBuffer, Entity e) =>
            {
                StringBuilder builder = new StringBuilder("Command list:\n");
                foreach(var command in commands.Values)
                    builder.Append($"/{command.CommandId.GetColorfulString(ChatConfig.Instance.HighlightColor)} {command.CommandFormat} - {command.CommandDescription}\n"); 
                return builder.ToString();
            },CommandType.ClientSide));            
            Add(new DebugCommand<CommandArg>("help", "Prints a description of the command","Command Name",(ref EntityCommandBuffer entityCommandBuffer,Entity e,CommandArg value) =>
            {
                if(commands.TryGetValue(value.CommandID,out var command))
                    return $"/{command.CommandId.GetColorfulString(ChatConfig.Instance.HighlightColor)} {command.CommandFormat}\n{command.CommandDescription}";
                return "Command not found";
            },commandType: CommandType.ClientSide));
            #endregion
            return commands;
        }
    }
    private static void Add(CommandBase command)
    {
        if(!commands.TryAdd(command.CommandId,command))
            Debug.LogWarning($"Command with ID '{command.CommandId}' already exists.");
    }
}