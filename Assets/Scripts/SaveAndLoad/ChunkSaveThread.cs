using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Unity.Collections;
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
                foreach(var data in chunks)
                {
                    data.Dispose();
                }
                chunks.Clear();
            }

           // ReadChunk(0,1);
        }
    }

    private static void WriteChunk(int regionIndex,List<ChunkData> chunks)
    {
        try
        {
            const int startOffset = 32;
            string regionName = Path.Combine(regionsPath,$"Region{regionIndex}");
            string pathCurrent =  $"{regionName}.bin";
            string pathTmp = $"{regionName}.tmp";
            string pathBak = $"{regionName}.old";

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
                 
            using var file = File.Open(pathTmp,FileMode.OpenOrCreate,FileAccess.ReadWrite);
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            using var reader = new BinaryReader(ms);


            
            if(file.Length > 0)
            {
                using(var br = new BinaryReader(file,Encoding.Default, leaveOpen: true))
                {  
                    file.Seek(0,SeekOrigin.Begin);
                    byte[] checkSum = br.ReadBytes(startOffset);
                    var compressed = br.ReadBytes((int)file.Length - 32); 
                    foreach(var k in checkSum)
                        Debug.Log(k);

                    Debug.Log("spane!!!");
                    foreach(var k in ComputeSHA256(compressed))
                        Debug.Log(k);  

                    if(!AreEqual(checkSum,ComputeSHA256(compressed)))
                    {
                        if (File.Exists(pathBak))
                        {
                            file.Close();
                            File.Replace(pathBak, pathCurrent,null);
                            WriteChunk(regionIndex,chunks);
                        }
                        throw new Exception("file is damaged");
                    }

                    Debug.Log("dzia!!");
                    try
                    {
                        writer.Write(Decompress(compressed));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("file is damaged");
                        if (File.Exists(pathBak))
                        {
                            file.Close();
                            File.Replace(pathBak, pathCurrent,null);
                            WriteChunk(regionIndex,chunks);
                        }
                        throw new Exception("file is damaged");
                    }

                    Debug.Log("dzia!kokok!");
                    ms.Flush();
                }
            }
        
   
            if (ms.Length < sizeof(int) * chunksCountInRegion * 2)        
            {
                ms.Seek(0,SeekOrigin.Begin);
                for (int i = 0; i < chunksCountInRegion; i++)
                {
                    writer.Write(0);
                    writer.Write(0);
                }
                ms.Flush();
            }

            (int offset,int size)[] offsets = new (int offset,int size)[chunksCountInRegion];
            ms.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < chunksCountInRegion; i++)
                offsets[i] = (reader.ReadInt32(),reader.ReadInt32());
            

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
                int oldSize = 0;
                if (chunkOffset.offset != 0)
                    oldSize = chunkOffset.size;

                if (chunkOffset.offset == 0 || newSize > oldSize)
                {
                    chunkOffset.offset = (int)ms.Length;
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
            ms.Seek(0,SeekOrigin.Begin);
            file.Seek(0,SeekOrigin.Begin);

            using var fileWriter = new BinaryWriter(file);
            var compressedRegion = Compress(reader.ReadBytes((int)ms.Length));    
            
            fileWriter.Write(ComputeSHA256(compressedRegion));
            fileWriter.Write(compressedRegion);  
            file.Flush();

            File.Replace(pathTmp,pathCurrent,pathBak);
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
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
           
            foreach(var item in chunk.tiles)
            {
                Debug.Log(item.tileID);
            }
            Debug.Log("space!!");
            foreach(var item in chunk.objects)
            {
                Debug.Log(item.id);
            }

            return chunk;
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
        return null;
    }

    public static byte[] SerializeChunk(ChunkData chunk)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(chunk.chunkIndex);
        bw.Write(chunk.tiles.Length); 
        byte[] tilesBytes = NativeArraySerializer.ToBytes(chunk.tiles);
        bw.Write(tilesBytes);
        bw.Write(chunk.objects.Length);
        byte[] objectsBytes = NativeArraySerializer.ToBytes(chunk.objects);
        bw.Write(objectsBytes);

        bw.Flush();
        return ms.ToArray();
    }
    private static byte[] Compress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
        {
            gzip.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
    private static byte[] Decompress(byte[] compressed)
    {
        using var ms = new MemoryStream(compressed);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var outStream = new MemoryStream();
        gzip.CopyTo(outStream);
        return outStream.ToArray();
    }
    public static byte[] ComputeSHA256(byte[] data)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(data);
    }

    private static bool AreEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;

        return true;
    }
}
