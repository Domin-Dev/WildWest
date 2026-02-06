using UnityEngine;
using System.IO;
using System.Text;
using System;
using UnityEditor.Localization.Plugins.XLIFF.V20;

public abstract class DoubleIndexedDataSaver<Data,FileIndex,ReadingIndex> : IndexedDataSaver<Data,FileIndex> where Data : unmanaged
{
    protected DoubleIndexedDataSaver(string directoryPath,string extension = "dat") : base(directoryPath,extension){}
    protected abstract bool Reading(MemoryStream ms, BinaryReader reader, FileIndex fileIndex, ReadingIndex index, out Data data);
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
}