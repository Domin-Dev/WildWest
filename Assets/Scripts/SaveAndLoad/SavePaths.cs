


using System.IO;
using UnityEngine;






public static class SavePaths
{
    public static string savesPath { get { return Path.Combine(Application.persistentDataPath, "Saves"); } }
    public static string settingsPath = Application.persistentDataPath + "/settings.json"; 
    public static string controlsPath = Application.persistentDataPath + "/controls.json"; 

    public const string headerFileName = "header";
    public const string headerExtension = "dan";
    public const string playerDataExtension = "dat";

    public const string playersDirectoryName = "Players";
    public const string regionsDirectoryName = "Regions";



    public static string GetHeaderPath(string worldFolder)
    {
        return Path.Combine(worldFolder, headerFileName + "." + headerExtension);
    }
    public static string GetMapFolder(string worldName)
    {
        return Path.Combine(GetWorldPath(worldName),"Maps");
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
        return Path.Combine(worldPath,playersDirectoryName);
    }
    public static string GetPlayerDataPath(string worldFolder,string playerName)
    {
        return Path.Combine(GetPlayersFolderByWorldName(worldFolder), playerName + "." + playerDataExtension);
    }
    public static string GetRegionsPath(string worldName)
    {
        return Path.Combine(GetWorldPath(worldName),regionsDirectoryName);
    }
}