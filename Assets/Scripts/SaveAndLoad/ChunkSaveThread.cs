using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public static class ChunkSaveIOThread
{
    private static Thread thread;
    private static bool running;
    private static AutoResetEvent signal = new AutoResetEvent(false);


    private static string regionsPath;

    private static int chunksCountInRegion;




    public static void Start(int chunksCountInReg)
    {
        if (running) return;

        if(GameInfo.instance != null && !string.IsNullOrEmpty(GameInfo.instance.worldName))
        {
            regionsPath = SaveSystem.GetRegionsPath(GameInfo.instance?.worldName);
            if (!Directory.Exists(regionsPath)) Directory.CreateDirectory(regionsPath);
        }
        chunksCountInRegion = chunksCountInReg;
        running = true;
        thread = new Thread(Loop)
        {
            IsBackground = true,
            Name = "Chunk Save IO Thread"
        };
        thread.Start();
    }

    public static void Stop()
    {
        running = false;
        signal.Set();
        thread?.Join();
    }


    public static void Notify()
    {
        signal.Set();
    }

    private static void Loop()
    {
        while (running)
        {
            signal.WaitOne(); 

            List<ChunkData> chunks = new List<ChunkData>();
            while(SavingChunksServerSystem.chunksToSaveRO.Length > 0)
            {
                int last = SavingChunksServerSystem.chunksToSaveRO.Length - 1;
                int region = SavingChunksServerSystem.chunksToSaveRO[last].region;
                for(int i = last; i >= 0; i--)
                {
                    var data = SavingChunksServerSystem.chunksToSaveRO[i];
                    if(data.region == region)
                    {
                        SavingChunksServerSystem.chunksToSaveRO.RemoveAt(i);
                        chunks.Add(data.chunk);
                    }
                }
                WriteChunk(region,chunks);
                chunks.Clear();
            }

            ReadChunk(0,0);
        }
    }

    private static void WriteChunk(int regionIndex,List<ChunkData> chunks)
    {
        try
        {
            string path = Path.Combine(regionsPath,$"Region{regionIndex}.bin");
            using var file = File.Open(path,FileMode.OpenOrCreate,FileAccess.ReadWrite);
            using var bw = new BinaryWriter(file);

            if (file.Length < sizeof(int) * chunksCountInRegion)
            {
                for (int i = 0; i < chunksCountInRegion; i++)
                {
                    bw.Write(0);
                    bw.Write(0);
                }
                bw.Flush();
            }

            (int offset,int size)[] offsets = new (int offset,int size)[chunksCountInRegion];
            file.Seek(0, SeekOrigin.Begin);
            using (var br = new BinaryReader(file, System.Text.Encoding.Default, leaveOpen: true))
            {
                for (int i = 0; i < chunksCountInRegion; i++)
                    offsets[i] = (br.ReadInt32(),br.ReadInt32());
            }

            foreach(var data in chunks)
            {
                byte[] chunkBytes;
                using (var ms = new MemoryStream())
                using (var chunkWriter = new BinaryWriter(ms))
                {
                    chunkWriter.Write(data.chunkIndex);
                    chunkWriter.Write(data.tiles.Length);
                    chunkWriter.Write(NativeArraySerializer.ToBytes(data.tiles));

                    chunkWriter.Write(data.objects.Length);
                    chunkWriter.Write(NativeArraySerializer.ToBytes(data.objects));

                    chunkBytes = ms.ToArray();
                }
                int newSize = chunkBytes.Length;


                var chunkOffset = offsets[data.localChunkIndex];
                int oldSize = 0;
                if (chunkOffset.offset != 0)
                    oldSize = chunkOffset.size;

                if (chunkOffset.offset == 0 || newSize > oldSize)
                {
                    chunkOffset.offset = (int)file.Length;
                    chunkOffset.size = newSize;
                    offsets[data.localChunkIndex] = chunkOffset;
                }

                file.Seek(data.localChunkIndex * 2 * sizeof(int), SeekOrigin.Begin);
                bw.Write(chunkOffset.offset);
                bw.Write(chunkOffset.size);

                file.Seek(chunkOffset.offset, SeekOrigin.Begin);
                bw.Write(chunkBytes);
                bw.Flush();
            }
        }
        finally
        {
            foreach(var data in chunks)
            {
                data.Dispose();
            }
        }
    }
    private static ChunkData? ReadChunk(int region,int localChunkIndex)
    {
        try
        {
            string path = Path.Combine(regionsPath,$"Region{region}.bin");
            if(!File.Exists(path)) return null;

            using var file = File.Open(path,FileMode.Open,FileAccess.Read);
            using var br = new BinaryReader(file, System.Text.Encoding.Default, leaveOpen: true);

            (int offset,int size)[] offsets = new (int offset,int size)[chunksCountInRegion];
            for (int i = 0; i < chunksCountInRegion; i++)
            {
                offsets[i] = (br.ReadInt32(),br.ReadInt32());
                Debug.Log("Chunk   " + offsets[i].offset + " " + offsets[i].size);
            }

            
            var offsetAndSize = offsets[localChunkIndex];
            if(offsetAndSize.offset == 0)
                return null;

            file.Seek(offsetAndSize.offset,SeekOrigin.Begin);
            

            ChunkData chunk = new ChunkData();
            chunk.chunkIndex = br.ReadInt32(); 
            int len = br.ReadInt32(); 
            chunk.tiles = NativeArraySerializer.FromBytes<TileData>(br.ReadBytes(len * Marshal.SizeOf<TileData>()),Allocator.Persistent);
            len = br.ReadInt32(); 
            chunk.objects = NativeArraySerializer.FromBytes<BuildingObjectData>(br.ReadBytes(len * Marshal.SizeOf<BuildingObjectData>()),Allocator.Persistent);
            return chunk;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        return null;
    }
}
