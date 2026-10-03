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
using UnityEngine.InputSystem;


public static class SaveSystem
{
    public static void CreateFolders()
    {
        string folderPath = SavePaths.GetWorldPath(GameInfo.instance.worldName);
        if(!Directory.Exists(SavePaths.savesPath))
            Directory.CreateDirectory(SavePaths.savesPath);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
    }
    public static void Save()
    {
          Debug.Log("stop!!");
        SaveIOThread.Stop();
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SavingServerSystem>().Save();
         Debug.Log("start!!");
        SaveIOThread.Start();
                 Debug.Log("stop!!");
        SaveIOThread.Stop();
    }  
   
   
   
    public static void SaveSettings(SettingsData Data, InputActionAsset Controls)
    {
        SaveJson(Data,SavePaths.settingsPath);
        SaveJson(Controls.SaveBindingOverridesAsJson(), SavePaths.controlsPath);
    }
    public static void SaveJson(object data, string path)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }
    public static void SaveJson(string data, string path)
    {
        File.WriteAllText(path, data);
    }
}
