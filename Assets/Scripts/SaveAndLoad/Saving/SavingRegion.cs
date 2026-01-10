
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;

public class SavingRegion : SavingBase<int, ChunkSave, int>
{
    private int chunksCountInRegion;
    private int offsetsSize => sizeof(int) * chunksCountInRegion * 2;
    private float defragmentationLimit;


    public SavingRegion(string regionsPath,int chunksCountInRegion,float defragmentationLimit) : base(regionsPath)
    {
        this.chunksCountInRegion = chunksCountInRegion;
        this.defragmentationLimit = defragmentationLimit + 1;
    }
    public override void GetFiles(int value, out string pathBak, out string pathTmp, out string pathCurrent)
    {
        string file = Path.Combine(directoryPath,$"Region{value}");
        GetPaths(file,"bin",out pathBak, out pathTmp, out pathCurrent);
    }
    public override void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer, int fileIndex, params ChunkSave[] chunks)
    {
        if (ms.Length < offsetsSize)        
            WriteChunkOffsets(ms,writer);

        (int offset,int size)[] offsets = new (int offset,int size)[chunksCountInRegion];
        ms.Seek(0, SeekOrigin.Begin);
        int size = 0;
        for (int i = 0; i < chunksCountInRegion; i++)
        {
            int offset = reader.ReadInt32();
            int chunkSize = reader.ReadInt32();
            offsets[i] = (offset,chunkSize);
            size += chunkSize;
        }
  
        foreach(var data in chunks)
        {
            byte[] chunkBytes;
            using (var chunkMem = new MemoryStream())
            using (var chunkWriter = new BinaryWriter(chunkMem))
            {
                chunkWriter.Write(data.chunkIndex);
                chunkWriter.Write(data.tiles.Length);
                chunkWriter.Write(NativeArraySerializer.ToBytes(data.tiles));
                chunkWriter.Write(data.objects.Length);
                chunkWriter.Write(NativeArraySerializer.ToBytes(data.objects));
                chunkBytes = chunkMem.ToArray();
            }
            int newSize = chunkBytes.Length;


            var chunkOffset = offsets[data.localChunkIndex];

            if (chunkOffset.offset == 0 || newSize > chunkOffset.size)
            {
                if(chunkOffset.offset +  chunkOffset.size != ms.Length) 
                    chunkOffset.offset = (int)ms.Length;

                size +=  newSize - chunkOffset.size;
                chunkOffset.size = newSize;
                offsets[data.localChunkIndex] = chunkOffset;
            }

            ms.Seek(data.localChunkIndex * 2 * sizeof(int), SeekOrigin.Begin);
            writer.Write(chunkOffset.offset);
            writer.Write(chunkOffset.size);

            ms.Seek(chunkOffset.offset, SeekOrigin.Begin);
            writer.Write(chunkBytes);
        }
        ms.Flush();


        if((ms.Length - offsetsSize)/(float)size > defragmentationLimit)
        {
            Defragmentation(offsets,ms,reader,writer);
        }
    }
    public override bool Reading(MemoryStream ms, BinaryReader reader, int fileIndex, int localChunkIndex, out ChunkSave data)
    {
        data = default;
        ms.Seek(0,SeekOrigin.Begin);
        (int offset,int size)[] offsets = new (int offset,int size)[chunksCountInRegion];
        for (int i = 0; i < chunksCountInRegion; i++)
            offsets[i] = (reader.ReadInt32(),reader.ReadInt32());
        
        var offsetAndSize = offsets[localChunkIndex];
        if(offsetAndSize.offset == 0)
            return false;

        ms.Seek(offsetAndSize.offset,SeekOrigin.Begin); 
        data = new ChunkSave();
        data.chunkIndex = reader.ReadInt32(); 
        int len = reader.ReadInt32(); 
        data.tiles = NativeArraySerializer.FromBytes<TileSave>(reader.ReadBytes(len * Marshal.SizeOf<TileSave>()),Allocator.Persistent);
        len = reader.ReadInt32(); 
        data.objects = NativeArraySerializer.FromBytes<BuildingObjectSave>(reader.ReadBytes(len * Marshal.SizeOf<BuildingObjectSave>()),Allocator.Persistent);
        return true;
    }
    public override bool Reading(MemoryStream ms, BinaryReader reader, int fileIndex, out ChunkSave data)
    {
        throw new System.NotImplementedException();
    }


    
    
    
    private void Defragmentation((int offset,int size)[] offsets,MemoryStream ms,BinaryReader readerMS, BinaryWriter writerMS)
    {
        ms.Seek(0,SeekOrigin.Begin);
        using (var newRegion = new MemoryStream())
        using (var chunkWriter = new BinaryWriter(newRegion))
        {
            WriteChunkOffsets(newRegion,chunkWriter);
            for(int i = 0; i < chunksCountInRegion;i++)
            {
                var offset = offsets[i];
                if(offset.offset > 0 && offset.size > 0)
                {
                    newRegion.Seek(i * 2 * sizeof(int), SeekOrigin.Begin);
                    chunkWriter.Write(newRegion.Length);
                    chunkWriter.Write(offset.size);
                    ms.Seek(offset.offset ,SeekOrigin.Begin);
                    newRegion.Seek((int)newRegion.Length,SeekOrigin.Begin);

                    chunkWriter.Write(readerMS.ReadBytes(offset.size));
                }
            }
            newRegion.Flush();
            ms.SetLength(0);
            ms.Seek(0,SeekOrigin.Begin);
            newRegion.Seek(0,SeekOrigin.Begin);
            newRegion.CopyTo(ms);
        }
    }
    private void WriteChunkOffsets(MemoryStream ms,BinaryWriter bw)
    {
        ms.Seek(0,SeekOrigin.Begin);
        for (int i = 0; i < chunksCountInRegion; i++)
        {
            bw.Write(0);
            bw.Write(0);
        }
        ms.Flush();
    }


}