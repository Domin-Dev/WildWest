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


public static class WorldManager
{
    public static bool WorldExist(string name)
    {
        var files = Directory.GetDirectories(SaveSystem.savesPath);

        foreach (var item in files)
        {
            if (Path.GetFileName(item) == name) return true;
        }
        return false;
    }
    public static void RemoveWorld(string name)
    {
        string file = Path.Combine(SaveSystem.savesPath, name);
        if (Directory.Exists(file))
        {
            Directory.Delete(file, true);
        }
    }

    public static bool ChangeName(string oldName, string newName)
    {
        string oldPath = Path.Combine(SaveSystem.savesPath, oldName);
        string newPath = Path.Combine(SaveSystem.savesPath, newName);
        if (Directory.Exists(oldPath) && !Directory.Exists(newPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream fileStream = new FileStream(SaveSystem.GetHeaderPath(oldPath), FileMode.Open);
            HeaderData headerData = formatter.Deserialize(fileStream) as HeaderData;
            fileStream.Close();

            headerData.worldName = newName;

            fileStream = new FileStream(SaveSystem.GetHeaderPath(oldPath), FileMode.Create);
            formatter.Serialize(fileStream, headerData);    
            fileStream.Close();

            Directory.Move(oldPath, newPath);
            return true;
        }
        return false;
    }
}
