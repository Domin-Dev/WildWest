using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;


public static class WorldManager
{
    public static bool WorldExist(string name)
    {
        if (Directory.Exists(SavePaths.savesPath))
        {
            var files = Directory.GetDirectories(SavePaths.savesPath);

            foreach (var item in files)
            {
                if (Path.GetFileName(item) == name) return true;
            }
        }
        return false;
    }
    public static void RemoveWorld(string name)
    {
        if(string.IsNullOrEmpty(name)) return;
        string file = SavePaths.GetWorldPath(name);
        Debug.Log(file);
        if (Directory.Exists(file))
        {
            Directory.Delete(file, true);
        }
    }
    public static bool ChangeName(string oldName, string newName)
    {
        if(oldName == newName) return true;
        string oldPath = Path.Combine(SavePaths.savesPath, oldName);
        string newPath = Path.Combine(SavePaths.savesPath, newName);

        if (Directory.Exists(oldPath) && !Directory.Exists(newPath))
        {
            if(SaveIOThread.TryLoadHeader(oldName,out HeaderSave header))
            {
                header.worldName = (FixedString128Bytes)newName;
                if(SaveIOThread.SaveHeader(oldName,header))
                {
                    Directory.Move(oldPath, newPath);
                    return true;
                }
            }
        }
        return false;
    }
}
