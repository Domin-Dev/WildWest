using UnityEngine;
using System.IO;
using System.Text;
using Unity.Mathematics;
using System.Security.Cryptography;
using System;
using System.IO.Compression;

public abstract class DataSaverRoot
{
    protected int startOffset = 16;
    protected string directoryPath;
    protected string extension;

    protected DataSaverRoot(string directoryPath,string extension = "dat")
    {
        this.directoryPath = directoryPath;
        this.extension = extension;    
            if(!Directory.Exists(directoryPath)) 
        Directory.CreateDirectory(directoryPath);
    }
    protected virtual void GetPaths(string fileName,out string pathBak,out string pathTmp,out string pathCurrent)
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

    protected bool ValidateFile(FileStream file,MemoryStream ms,string pathCurrent, string pathBak)
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
                        Debug.Log("bladkkkkkk!!!");
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
                        Debug.Log("blad!!!");
                        return false;
                    }
                    throw new Exception("file is damaged");
                }
                ms.Flush();
            }
        }
        return true;
    }
    protected byte[] Compress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
        {
            gzip.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
    protected byte[] Decompress(byte[] compressed)
    {
        using var ms = new MemoryStream(compressed);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var outStream = new MemoryStream();
        gzip.CopyTo(outStream);
        return outStream.ToArray();
    }
    protected byte[] HashMD5(byte[] data)
    {
        using var mD5 = MD5.Create();
        return mD5.ComputeHash(data);
    }
    protected bool AreEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;

        return true;
    }

}