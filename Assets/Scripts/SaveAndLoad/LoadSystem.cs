using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class LoadSystem
{ 
    public static List<HeaderData> LoadHeaders()
    {
        List<HeaderData> headers = new List<HeaderData>();
        if (!Directory.Exists(SaveSystem.savesPath)) return null;

        var files = Directory.GetDirectories(SaveSystem.savesPath);
        BinaryFormatter formatter = new BinaryFormatter();

        foreach (var file in files)
        {
            try
            {
                string path = SaveSystem.GetHeaderPath(file);
                if (!File.Exists(path) || new FileInfo(path).Length == 0) continue;
                FileStream fileStream = new FileStream(path, FileMode.Open);
                HeaderData data = formatter.Deserialize(fileStream) as HeaderData;

                if(data == null) continue;  
                headers.Add(data);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        return headers;
    }
    public static HeaderData LoadHeader(string worldName)
    {
        string worldPath = SaveSystem.GetWorldPath(worldName);
        if (!Directory.Exists(worldPath)) return null;
        BinaryFormatter formatter = new BinaryFormatter();
        HeaderData headerData = null;

        try
        {
            string path = SaveSystem.GetHeaderPath(worldPath);
            if (!File.Exists(path) || new FileInfo(path).Length == 0) return null; 
            FileStream fileStream = new FileStream(path, FileMode.Open);
            headerData = formatter.Deserialize(fileStream) as HeaderData;
            fileStream.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
        

        return headerData;
    }
    public static PlayerSave LoadPlayerSave(string worldName,string playerName)
    {
        if(worldName == string.Empty) return null;

        string path = SaveSystem.GetPlayerDataPath(worldName, playerName);
        Debug.Log(path);
        BinaryFormatter formatter = new BinaryFormatter();

        if (!File.Exists(path) || new FileInfo(path).Length == 0) return null;
        FileStream fileStream = new FileStream(path, FileMode.Open);

        PlayerSave playerSave = formatter.Deserialize(fileStream) as PlayerSave;

        fileStream.Close();
        return playerSave;
    }
    public static PlayerSave LoadPlayerSave(string playerName)
    {
       return LoadPlayerSave(GameInfo.instance.worldName,playerName);
    }
    public static T LoadJson<T>(string path) 
    {
        T Data = default(T);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Data = JsonUtility.FromJson<T>(json);
        }
        return Data;
    }
   
    public static SettingsData LoadSettings(out string controls)
    {
        Debug.Log("load!!");
        controls = LoadJson<string>(SaveSystem.controlsPath);
        return LoadJson<SettingsData>(SaveSystem.settingsPath);

    }



}


