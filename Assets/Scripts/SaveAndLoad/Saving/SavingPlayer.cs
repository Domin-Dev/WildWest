
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;

public class SavingPlayer : SavingBase<string, PlayerSave, int>
{
    public SavingPlayer(string playersPath) : base(playersPath)
    {
        
    }
    public override void GetFiles(string playerName, out string pathBak, out string pathTmp, out string pathCurrent)
    {
        string file = Path.Combine(directoryPath,playerName);
        GetPaths(file,"dat",out pathBak, out pathTmp, out pathCurrent);
    }

    public override void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer, string fileIndex, params PlayerSave[] data)
    {
        ms.Seek(0,SeekOrigin.Begin);
        writer.Write(NativeArraySerializer.StructToBytes(data[0]));
    }
    public override bool Reading(MemoryStream ms, BinaryReader reader, string fileIndex, int index, out PlayerSave data)
    {
        ms.Seek(0,SeekOrigin.Begin);
        data = NativeArraySerializer.BytesToStruct<PlayerSave>(reader.ReadBytes(Marshal.SizeOf<PlayerSave>()));
        return true;
    }

    public override bool Reading(MemoryStream ms, BinaryReader reader, string fileIndex, out PlayerSave data)
    {
        data = default;
        return true;
    }
}