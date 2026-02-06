using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Runtime.InteropServices;

public class IndexedDataSaver<Data,FileIndex> : DataSaverRoot where Data : unmanaged
{
    public IndexedDataSaver(string directoryPath,string extension = "dat") : base(directoryPath,extension){}

    protected virtual void GetFiles(FileIndex value, out string pathBak, out string pathTmp, out string pathCurrent)
    {
        string file = Path.Combine(directoryPath,value.ToString());
        GetPaths(file,out pathBak, out pathTmp, out pathCurrent);
    }
    protected virtual void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer,FileIndex fileIndex, params Data[] data)
    {
        ms.Seek(0,SeekOrigin.Begin);
        writer.Write(NativeArraySerializer.StructToBytes(data[0]));
    }
    protected virtual bool Reading(MemoryStream ms, BinaryReader reader, FileIndex fileIndex, out Data data)
    {
        ms.Seek(0,SeekOrigin.Begin);
        data = NativeArraySerializer.BytesToStruct<Data>(reader.ReadBytes(Marshal.SizeOf<Data>()));
        return true;
    }

    public void StartWriting(FileIndex fileIndex,params Data[] data)
    {
        try
        {
            GetFiles(fileIndex,out string pathBak,out string pathTmp,out string pathCurrent);
            CheckWriteFiles(pathBak,pathTmp,pathCurrent);

            using var file = File.Open(pathTmp,FileMode.OpenOrCreate,FileAccess.ReadWrite);
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            using var reader = new BinaryReader(ms);

            if(!ValidateFile(file,ms,pathCurrent,pathBak))
            {
                StartWriting(fileIndex,data);
                return;
            }
            Writing(ms,reader,writer,fileIndex,data);

            ms.Seek(0,SeekOrigin.Begin);
            file.Seek(0,SeekOrigin.Begin);

            using var fileWriter = new BinaryWriter(file);
            var compressed = Compress(reader.ReadBytes((int)ms.Length));      
            fileWriter.Write(HashMD5(compressed));
            fileWriter.Write(compressed);  
            file.Flush();
            File.Replace(pathTmp,pathCurrent,pathBak);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }   
    public bool StartReading(FileIndex fileIndex,out Data data)
    {
        data = default;
        try
        {
            GetFiles(fileIndex,out string pathBak,out string pathTmp,out string pathCurrent);
            if(!CheckReadFiles(pathBak,pathCurrent)) return false;
        
            using var file = File.Open(pathCurrent,FileMode.Open,FileAccess.Read);
            using var ms = new MemoryStream();
            using var reader = new BinaryReader(ms);

            if(!ValidateFile(file,ms,pathCurrent,pathBak)) 
            {
                return StartReading(fileIndex,out data);
            }
            file.Close();
            return Reading(ms,reader,fileIndex,out data);
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
        return false;
    }
}