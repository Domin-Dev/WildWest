using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Unity.Collections;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.UIElements;

public static class SaveIOThread
{
    private static Thread thread;
    private static bool running;
    private static AutoResetEvent signal = new AutoResetEvent(false);
    private const int startOffset = 32;



    private static string playersPath;


    private static SavingRegion savingRegion;
    private static SavingPlayer savingPlayer;

    public static void Start(int chunksCountInRegion,float defragmentationLimit)
    {
        if (running) return;

        Debug.Log("dzkoaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        if(GameInfo.instance != null && !string.IsNullOrEmpty(GameInfo.instance.worldName))
        {
            string workName = GameInfo.instance?.worldName;
            SaveSystem.CreateFolders();

            savingRegion = new SavingRegion(SaveSystem.GetRegionsPath(workName),chunksCountInRegion,defragmentationLimit);
            savingPlayer = new SavingPlayer(SaveSystem.GetPlayersFolderByWorldName(workName));
        }
        else 
            return;


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


            savingRegion.StartReading(0,0,out var data);

            foreach(var i in data.tiles)
            {
                Debug.Log(i.tileID);
            }
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
            savingRegion.StartWriting(region,chunks.ToArray());
            foreach(var data in chunks)
            {
                data.Dispose();
            }
            chunks.Clear();
        }
    }
    private static void PlayerSaving()
    {
        while(SavingServerSystem.playersToSaveRO.Length > 0)
        {
            foreach(var player in SavingServerSystem.playersToSaveRO)
            {
                savingPlayer.StartWriting(player.playerName.ToString(),player);
            }
        }
    }


   
   

   

}
