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

            using var file = File.Open(pathTmp,FileMode.Create,FileAccess.ReadWrite);
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
            
            if(file.Position < file.Length)
                file.SetLength(file.Position);
          
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

public class IndexedDataSaver<Data1,Data2,FileIndex> : DataSaverRoot
 where Data1 : unmanaged
 where Data2 : unmanaged
{
    public IndexedDataSaver(string directoryPath,string extension = "dat") : base(directoryPath,extension){}
    protected virtual void GetFiles(FileIndex value, out string pathBak, out string pathTmp, out string pathCurrent)
    {
        string file = Path.Combine(directoryPath,value.ToString());
        GetPaths(file,out pathBak, out pathTmp, out pathCurrent);
    }
    protected virtual void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer,FileIndex fileIndex, Data1[] data1,params Data2[] data2) 
    {
        ms.Seek(0,SeekOrigin.Begin);
        writer.Write(NativeArraySerializer.StructToBytes(data1[0]));
        writer.Write(NativeArraySerializer.StructToBytes(data2[0]));
    }
    protected virtual bool Reading(MemoryStream ms, BinaryReader reader, FileIndex fileIndex, out Data1[] data1, out Data2[] data2)
    {
        ms.Seek(0,SeekOrigin.Begin);
        data1 = new Data1[] {NativeArraySerializer.BytesToStruct<Data1>(reader.ReadBytes(Marshal.SizeOf<Data1>()))};
        data2 = new Data2[] {NativeArraySerializer.BytesToStruct<Data2>(reader.ReadBytes(Marshal.SizeOf<Data2>()))};
        return true;
    }
    public void StartWriting(FileIndex fileIndex,Data1[] data1, params Data2[] data2)
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
                StartWriting(fileIndex,data1,data2);
                return;
            }
            Writing(ms,reader,writer,fileIndex,data1,data2);

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
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }   
    public bool StartReading(FileIndex fileIndex,out Data1[] data1, out Data2[] data2)
    {
        data1 = default;
        data2 = default;
        try
        {
            GetFiles(fileIndex,out string pathBak,out string pathTmp,out string pathCurrent);
            if(!CheckReadFiles(pathBak,pathCurrent)) return false;
        
            using var file = File.Open(pathCurrent,FileMode.Open,FileAccess.Read);
            using var ms = new MemoryStream();
            using var reader = new BinaryReader(ms);

            if(!ValidateFile(file,ms,pathCurrent,pathBak)) 
            {
                return StartReading(fileIndex,out data1,out data2);
            }
            file.Close();
            return Reading(ms,reader,fileIndex,out data1,out data2);
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
        return false;
    }

} 