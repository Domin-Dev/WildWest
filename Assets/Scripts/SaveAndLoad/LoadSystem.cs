using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LoadSystem
{ 
    public static List<(HeaderSave header, PlayerSave playerData)> LoadHeaders()
    {
        List<(HeaderSave,PlayerSave)> headers = new List<(HeaderSave,PlayerSave)>();
        if (!Directory.Exists(SavePaths.savesPath)) return null;
        var files = Directory.GetDirectories(SavePaths.savesPath);

        foreach (var file in files)
        {
            try
            {
                string worldName = Path.GetFileName(file);
                if(SaveIOThread.TryLoadHeader(worldName,out HeaderSave header))
                {
                    if(SaveIOThread.TryLoadPlayer(header.playerName.ToString(),worldName,out PlayerSave playerData,out var containers))
                    {
                        headers.Add((header,playerData));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        return headers;
    }
    public static T LoadJson<T>(string path) where T : class
    {
        T Data = null;
        Debug.Log("null jest");
        if (File.Exists(path))
        {
            Debug.Log("exist!");
            string json = File.ReadAllText(path);
            Data = JsonUtility.FromJson<T>(json);
        }
        return Data;
    }
    public static string LoadText(string path) 
    {
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return null;
    }
    public static void LoadSettings(out string controls, out SettingsData settings)
    {
        controls = LoadText(SavePaths.controlsPath);
        settings = LoadJson<SettingsData>(SavePaths.settingsPath);
    }
}


