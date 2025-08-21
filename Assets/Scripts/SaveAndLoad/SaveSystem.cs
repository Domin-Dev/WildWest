using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
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

    public static string GetHeaderPath(string worldFolder)
    {
        return Path.Combine(worldFolder, "header.dan");
    }

    public static string GetWorldPath(string worldName)
    {
        return Path.Combine(savesPath, worldName);
    }

    public static void Save()
    {
        Dictionary<string, PlayerSave> players = GetPlayers(out PlayerSave hostPlayer);
        Debug.Log(hostPlayer + " ttto!");

        BinaryFormatter formatter = new BinaryFormatter();

        Debug.Log(savesPath);
        string folderPath = Path.Combine(savesPath, GameInfo.instance.worldName);
        Debug.Log(folderPath);


        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        string worldPath = Path.Combine(folderPath, "world.dust");
        FileStream stream = new FileStream(worldPath, FileMode.Create);
        formatter.Serialize(stream, new PlayerSave());

        SaveHeader(folderPath, hostPlayer);


        string playersPath = Path.Combine(folderPath, "Players");
        if (!Directory.Exists(playersPath))
            Directory.CreateDirectory(playersPath);
        SavePlayers(playersPath,players);

        
        stream.Close();
    }
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
        var players = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        Dictionary<string,PlayerSave> playersToSave = new Dictionary<string, PlayerSave>();

        foreach (var entity in players)
        {
            PlayerSave playerSave = new PlayerSave();

            var playerData = entityManager.GetComponentData<Player>(entity);
            var playerLook = entityManager.GetComponentData<PlayerLook>(entity);
            playerSave.playerName = playerData.playerName;
            playerSave.characterLook = playerLook.look;

            if (entityManager.HasComponent<GhostOwnerIsLocal>(entity))
            {
                hostPlayer = playerSave;
            }
            playersToSave.Add(playerSave.playerName.ToString(),playerSave);
        }

        players.Dispose();
        return playersToSave;
    }
}
