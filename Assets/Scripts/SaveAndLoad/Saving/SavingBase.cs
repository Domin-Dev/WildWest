using UnityEngine;
using System.IO;
using System.Text;
using Unity.Mathematics;
using System.Security.Cryptography;
using System;
using System.IO.Compression;

public abstract class SavingBase<FileIndex,Data,ReadingIndex> 
{

    protected int startOffset = 16;
    protected string directoryPath;

    protected SavingBase(string directoryPath)
    {
        this.directoryPath = directoryPath;
            if(!Directory.Exists(directoryPath)) 
        Directory.CreateDirectory(directoryPath);
    }
    public abstract void GetFiles(FileIndex value,out string pathBak,out string pathTmp,out string pathCurrent); 
    public abstract void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer,FileIndex fileIndex, params Data[] data);
    public abstract bool Reading(MemoryStream ms, BinaryReader reader, FileIndex fileIndex, ReadingIndex index, out Data data);
    public abstract bool Reading(MemoryStream ms, BinaryReader reader, FileIndex fileIndex, out Data data);


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
                StartWriting(fileIndex);
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
    public bool StartReading(FileIndex fileIndex,ReadingIndex index,out Data data)
    {
        data = default;
        try
        {
            GetFiles(fileIndex,out string pathBak,out string pathTmp,out string pathCurrent);
            if(!CheckReadFiles(pathBak,pathCurrent)) return default;
            
            using var file = File.Open(pathCurrent,FileMode.Open,FileAccess.Read);
            using var ms = new MemoryStream();
            using var reader = new BinaryReader(ms);

            if(!ValidateFile(file,ms,pathCurrent,pathBak)) return StartReading(fileIndex,index,out data);
            file.Close();
            return Reading(ms,reader,fileIndex,index, out data);
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
        return false;
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

            if(!ValidateFile(file,ms,pathCurrent,pathBak)) return StartReading(fileIndex,out data);
            file.Close();
            return Reading(ms,reader,fileIndex,out data);
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
        return false;
    }


    protected void GetPaths(string fileName,string extension,out string pathBak,out string pathTmp,out string pathCurrent)
    {
        pathCurrent = fileName + "." + extension;
        pathTmp = fileName + ".tmp";
        pathBak = fileName + ".old";
    }
    protected void CheckWriteFiles(string pathBak,string pathTmp,string pathCurrent)
    {
        if(!File.Exists(pathCurrent))
        {        
            if (File.Exists(pathBak))
            {
                File.Move(pathBak, pathCurrent);
                File.Copy(pathCurrent,pathTmp, true);
            }
            else 
                File.Create(pathCurrent).Close();
        }
        else
            File.Copy(pathCurrent,pathTmp, true);
    }
    protected bool CheckReadFiles(string pathBak,string pathCurrent)
    {
        if(!File.Exists(pathCurrent))
        {        
            if (File.Exists(pathBak))
                File.Move(pathBak, pathCurrent);          
            else
                return false;
        }
        return true;
    }

    private bool ValidateFile(FileStream file,MemoryStream ms,string pathCurrent, string pathBak)
    {
        if(file.Length > 0)
        {
            using(var br = new BinaryReader(file,Encoding.Default, leaveOpen: true))
            {  
                file.Seek(0,SeekOrigin.Begin);
                byte[] checkSum = br.ReadBytes(startOffset);
                var compressed = br.ReadBytes((int)file.Length - startOffset); 
                if(!AreEqual(checkSum,HashMD5(compressed)))
                {
                    if (File.Exists(pathBak))
                    {
                        file.Close();
                        File.Replace(pathBak, pathCurrent,null);
                        return false;
                    }
                    throw new Exception("file is damaged");
                }

                try
                {
                    using(var bw = new BinaryWriter(ms,Encoding.Default,true))
                    {   
                        bw.Write(Decompress(compressed));
                    }
                }
                catch (Exception e)
                {
                    if (File.Exists(pathBak))
                    {
                        file.Close();
                        File.Replace(pathBak, pathCurrent,null);
                        return false;
                    }
                    throw new Exception("file is damaged");
                }
                ms.Flush();
            }
        }
        return true;
    }
    private byte[] Compress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
        {
            gzip.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
    private byte[] Decompress(byte[] compressed)
    {
        using var ms = new MemoryStream(compressed);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var outStream = new MemoryStream();
        gzip.CopyTo(outStream);
        return outStream.ToArray();
    }
    private byte[] HashMD5(byte[] data)
    {
        using var mD5 = MD5.Create();
        return mD5.ComputeHash(data);
    }
    private bool AreEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;

        return true;
    }
}