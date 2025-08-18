using System;
using System.Collections.Generic;
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
                if (!File.Exists(path)) continue;
                FileStream fileStream = new FileStream(SaveSystem.GetHeaderPath(file), FileMode.Open);
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

}

