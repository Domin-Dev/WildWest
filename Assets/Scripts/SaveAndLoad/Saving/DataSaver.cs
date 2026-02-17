using UnityEngine;
using System.IO;
using System.Text;
using Unity.Mathematics;
using System.Security.Cryptography;
using System;
using System.IO.Compression;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

public class DataSaver<Data> : DataSaverRoot where Data : unmanaged
{
    protected string fileName;
    public DataSaver(string directoryPath,string fileName,string extension = "dat") : base(directoryPath,extension)
    {
        this.fileName = fileName;    
    }
    protected virtual void GetFiles(out string pathBak,out string pathTmp,out string pathCurrent)
    {
        string file = Path.Combine(directoryPath,fileName);
        GetPaths(file,out pathBak, out pathTmp, out pathCurrent);
    }
    protected virtual void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer,params Data[] data)
    {
        ms.Seek(0,SeekOrigin.Begin);
        foreach(var d in data)
            writer.Write(NativeArraySerializer.StructToBytes(d));
    }
    protected virtual bool Reading(MemoryStream ms, BinaryReader reader, out Data data)
    {
        ms.Seek(0,SeekOrigin.Begin);
        data = NativeArraySerializer.BytesToStruct<Data>(reader.ReadBytes(Marshal.SizeOf<Data>()));
        return true;
    }
    public bool StartWriting(params Data[] data)
    {
        try
        {
            GetFiles(out string pathBak,out string pathTmp,out string pathCurrent);
            CheckWriteFiles(pathBak,pathTmp,pathCurrent);

            using var file = File.Open(pathTmp,FileMode.OpenOrCreate,FileAccess.ReadWrite);
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            using var reader = new BinaryReader(ms);

            if(!ValidateFile(file,ms,pathCurrent,pathBak))
            {
                return StartWriting(data);
            }
            Writing(ms,reader,writer,data);

            ms.Seek(0,SeekOrigin.Begin);
            file.Seek(0,SeekOrigin.Begin);

            using var fileWriter = new BinaryWriter(file);
            var compressed = Compress(reader.ReadBytes((int)ms.Length));      
            fileWriter.Write(HashMD5(compressed));
            fileWriter.Write(compressed);  
            
            if(file.Position < file.Length)
                file.SetLength(file.Position);
                
            file.Flush();
            File.Replace(pathTmp,pathCurrent,pathBak);
            return true;
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        return false;
    }   
    public bool StartReading(out Data data)
    {
        data = default;
        try
        {
            GetFiles(out string pathBak,out string pathTmp,out string pathCurrent);
            if(!CheckReadFiles(pathBak,pathCurrent)) return false;
        
            Debug.Log(pathCurrent);
            using var file = File.Open(pathCurrent,FileMode.Open,FileAccess.Read, FileShare.Read);
            using var ms = new MemoryStream();
            using var reader = new BinaryReader(ms);

            if(!ValidateFile(file,ms,pathCurrent,pathBak)) 
            {
                Debug.Log("przywracanie backup!");
                return StartReading(out data);
            }
            file.Close();
            return Reading(ms,reader,out data);
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
        return false;
    }
}