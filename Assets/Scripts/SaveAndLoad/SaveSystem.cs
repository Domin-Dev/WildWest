using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


public static class SaveSystem
{

    public static string savesPath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "Saves");
        }
    }
    public static string settingsPath = Application.persistentDataPath + "/settings.json";


    public static string GetHeaderPath(string worldFolder)
    {
        return Path.Combine(worldFolder, "header.dan");
    }
    public static string GetWorldPath(string worldName)
    {
        return Path.Combine(savesPath, worldName);
    }
    public static string GetPlayersFolderByWorldName(string worldName)
    {
        return GetPlayersFolder(GetWorldPath(worldName));
    }
    public static string GetPlayersFolder(string worldPath)
    {
        return Path.Combine(worldPath, "Players");
    }
    public static string GetPlayerDataPath(string worldFolder,string playerName)
    {
        return Path.Combine(GetPlayersFolderByWorldName(worldFolder), playerName + ".dat");
    }




    #region Saves

    public static void Save()
    {
        Dictionary<string, PlayerSave> players = GetPlayers(out PlayerSave hostPlayer);
        BinaryFormatter formatter = new BinaryFormatter();
        string folderPath = GetWorldPath(GameInfo.instance.worldName);

        if(!Directory.Exists(savesPath))
            Directory.CreateDirectory(savesPath);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        string worldPath = Path.Combine(folderPath, "world.dust");
        FileStream stream = new FileStream(worldPath, FileMode.Create);
        formatter.Serialize(stream, new PlayerSave());

        SaveHeader(folderPath, hostPlayer);



        string playersPath = GetPlayersFolder(folderPath);
        if (!Directory.Exists(playersPath))
            Directory.CreateDirectory(playersPath);
        SavePlayers(playersPath,players);
        stream.Close();
    }
    public static void SaveSettings(SettingsData Data)
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(settingsPath, json);
    }

    #endregion





    private static void SaveHeader(string folderPath, PlayerSave playerSave)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string headerPath = GetHeaderPath(folderPath);
        FileStream stream = new FileStream(headerPath, FileMode.Create);
        HeaderData headerData = new HeaderData();
        headerData.playerName = playerSave.playerName;
        headerData.difficulty = GameInfo.instance.difficultyLevel;
        headerData.playTime = GameInfo.instance.playTime;


        headerData.characterLook = playerSave.characterLook;
        headerData.worldName = GameInfo.instance.worldName; 
        headerData.seed = GameInfo.instance.seed;   

        headerData.saveTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        headerData.creationTime = GameInfo.instance.creationTime;

        formatter.Serialize(stream, headerData);
        stream.Close();
    }   
    private static void SavePlayers(string folderPath, Dictionary<string, PlayerSave> players)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        foreach (var item in players)
        {
            string path = Path.Combine(folderPath, item.Key + ".dat");
            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, item.Value);
            stream.Close();
        }
    }
    private static Dictionary<string,PlayerSave> GetPlayers(out PlayerSave hostPlayer)
    {
        hostPlayer = null;
        var world = ClientServerBootstrap.ServerWorld;
        var entityManager = world.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(Player), typeof(Simulate));

        var queryNetworkID = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection));
        var array = queryNetworkID.ToEntityArray(Allocator.Temp);
        int hostID = ClientServerBootstrap.ClientWorld.EntityManager.GetComponentData<NetworkId>(array[0]).Value;


        var players = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        Dictionary<string,PlayerSave> playersToSave = new Dictionary<string, PlayerSave>();

        foreach (var entity in players)
        {
            PlayerSave playerSave = new PlayerSave();

            var playerData = entityManager.GetComponentData<Player>(entity);
            var source = entityManager.GetComponentData<PlayerSourceConnection>(entity);      

            var playerLook = entityManager.GetComponentData<PlayerLook>(entity);
            var pos = entityManager.GetComponentData<LocalTransform>(entity);

            var health = entityManager.GetComponentData<Health>(entity);
            var hunger = entityManager.GetComponentData<Hunger>(entity);
            var thirst = entityManager.GetComponentData<Thirst>(entity);


            playerSave.isAdmin = entityManager.HasComponent<Admin>(source.value);

            playerSave.playerName = playerData.playerName;
            playerSave.characterLook = playerLook.look;
            playerSave.playerPosition = new float2(pos.Position.x,pos.Position.y);

            playerSave.health = health.Value;
            playerSave.hunger = hunger.Value;
            playerSave.thirst = thirst.Value;



            if (entityManager.GetComponentData<GhostOwner>(entity).NetworkId == hostID)
            {
                hostPlayer = playerSave;
            }
            playersToSave.TryAdd(playerSave.playerName.ToString(),playerSave);
        }

        array.Dispose();
        query.Dispose();
        queryNetworkID.Dispose();
        players.Dispose();
        return playersToSave;
    }
}
