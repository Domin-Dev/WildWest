using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public static class SaveIOThread
{
    private static Thread thread;
    private static bool running;
    private static AutoResetEvent signal = new AutoResetEvent(false);
    private const int startOffset = 32;



    private static string playersPath;

    private static RegionSaver regionSaver;
    private static PlayerSaver playerSaver;
    private static DataSaver<HeaderSave> headerSaver;

    public static void Create(int chunksCountInRegion,float defragmentationLimit)
    {
        if (running) return;
        if(GameInfo.instance != null && !string.IsNullOrEmpty(GameInfo.instance.worldName))
        {
            string worldName = GameInfo.instance?.worldName;
            SaveSystem.CreateFolders();

            regionSaver = new RegionSaver(SavePaths.GetRegionsPath(worldName),chunksCountInRegion,defragmentationLimit);
            playerSaver = new PlayerSaver(SavePaths.GetPlayersFolderByWorldName(worldName));
            headerSaver = new DataSaver<HeaderSave>(SavePaths.GetWorldPath(GameInfo.instance.worldName),SavePaths.headerFileName,SavePaths.headerExtension);
        }
        else 
            return;

        Start();
    }
    public static void Start()
    {
        if(running) return;

        running = true;
        thread = new Thread(Loop)
        {
            IsBackground = true,
            Name = "Save IO Thread"
        };
        thread.Start();
    }
    public static void Stop()
    {
        if(!running) return;

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
            ChunkSaving();
            PlayerSaving();
            HeaderSaving();
        }
    }
    private static void ChunkSaving()
    {
        List<ChunkSave> chunks = new List<ChunkSave>();
        while(SavingServerSystem.chunksToSaveRO.Length > 0)
        {
            int last = SavingServerSystem.chunksToSaveRO.Length - 1;
            int region = SavingServerSystem.chunksToSaveRO[last].region;
            for(int i = last; i >= 0; i--)
            {
                var data = SavingServerSystem.chunksToSaveRO[i];
                if(data.region == region)
                {
                    SavingServerSystem.chunksToSaveRO.RemoveAt(i);
                    chunks.Add(data.chunk);
                }
            }
            regionSaver.StartWriting(region,chunks.ToArray());
            foreach(var data in chunks)
            {
                data.Dispose();
            }
            chunks.Clear();
        }
    }
    private static void PlayerSaving()
    {
        List<ContainerSave> containerSaves = new List<ContainerSave>(); 
        while (SavingServerSystem.containersToSaveRO.Length > 0)
        {
            int last = SavingServerSystem.containersToSaveRO.Length - 1;
            FixedString128Bytes player = SavingServerSystem.containersToSaveRO[last].playerName;
            for(int i = last; i >= 0; i--)
            {
                var data = SavingServerSystem.containersToSaveRO[i];
                if(data.playerName == player)
                {
                    SavingServerSystem.containersToSaveRO.RemoveAt(i);
                    containerSaves.Add(data.container);
                }
            }
            bool saved = false;
            for(int i = 0; i < SavingServerSystem.playersToSaveRO.Length; i--)
            {
                var data =SavingServerSystem.playersToSaveRO[i];
                if(data.playerName == player)
                {
                    playerSaver.StartWriting(player.ToString(),new[]{data},containerSaves.ToArray());
                    SavingServerSystem.playersToSaveRO.RemoveAtSwapBack(i);
                    saved = true;
                    break;
                }
            }

            if(!saved)
                playerSaver.StartWriting(player.ToString(),null,containerSaves.ToArray());

            foreach(var data in containerSaves)
                data.Dispose();  
            containerSaves.Clear();
        }


        foreach(var player in SavingServerSystem.playersToSaveRO)
            playerSaver.StartWriting(player.playerName.ToString(),new[]{player},null);
        
        SavingServerSystem.playersToSaveRO.Clear();
    }
    private static void HeaderSaving()
    {
        if(SavingServerSystem.headerDataRO.HasValue)
        {
            HeaderSave headerData = SavingServerSystem.headerDataRO.Value;
            headerData.saveTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            headerSaver.StartWriting(headerData);
            SavingServerSystem.headerDataRO = null;
        }
    }

    public static bool TryLoadHeader(string worldName,out HeaderSave headerData)
    {
        return new DataSaver<HeaderSave>(SavePaths.GetWorldPath(worldName),SavePaths.headerFileName,SavePaths.headerExtension).StartReading(out headerData);
    }
    public static bool SaveHeader(string worldName,HeaderSave headerData)
    {
        return new DataSaver<HeaderSave>(SavePaths.GetWorldPath(worldName),SavePaths.headerFileName,SavePaths.headerExtension).StartWriting(headerData);
    }    
    public static bool TryLoadHeader(out HeaderSave headerData)
    {
        headerData = default;
        if(headerSaver != null)
            return headerSaver.StartReading(out headerData);
        return false;
    }
   
    public static bool TryLoadPlayer(string playerName, string worldName, out PlayerSave playerData, out ContainerSave[] containers)
    {
        bool value = new PlayerSaver(SavePaths.GetPlayersFolderByWorldName(worldName),SavePaths.playerDataExtension).
            StartReading(playerName,out var player,out containers);
        if(value) 
            playerData = player[0];
        else
            playerData = default;

        return value;
    }
    public static bool TryLoadPlayer(string playerName, out PlayerSave playerData,out ContainerSave[] containers)
    {
        return TryLoadPlayer(playerName,GameInfo.instance.worldName,out playerData,out containers);
    }
}

