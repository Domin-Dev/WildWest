
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    List<object> commandList = new List<object>();
    public List<object> GetCommandList()
    {
        commandList.Add(new DebugCommand("set_time", "sets the time", "", () =>
        {
            Debug.Log("time set to 10");
        }));
        commandList.Add(new DebugCommand("host", "start host", "", () =>
        {
            NetworkManager.Singleton.StartHost();
        }));
        commandList.Add(new DebugCommand<int>("spawn", "spawn", "[Number]", (x) =>
        {
            Debug.Log("spawn" + x);
        }));
        commandList.Add(new DebugCommand<int,int>("give", "Adds item to player equipment", "[ID] [Count]", (ID, count) =>
        {
           if(ItemsAsset.instance.IsItem(ID)) EquipmentManager.instance.AddNewItem(ItemsAsset.instance.GetItemStats(ID,count));
           else ChatManager.instance.Print("ID is't correct");
        }));
        commandList.Add(new DebugCommand<int>("give", "Adds item to player equipment", "[ID]", (ID) =>
        {
            if (ItemsAsset.instance.IsItem(ID)) EquipmentManager.instance.AddNewItem(ItemsAsset.instance.GetItemStats(ID));
            else ChatManager.instance.Print("ID is't correct");
        }));
        commandList.Add(new DebugCommand<int>("water", "Prints information about water body", "[Water body ID]", (WaterBodyID) =>
        {
            string text;
            LiquidsManager.instance.waterBodies.TryGetValue(WaterBodyID, out WaterBody waterBody);
            if(waterBody != null)
                text = $"Water Body ID:{WaterBodyID} TileCount:{waterBody.tileCount} Fill:{Math.Round(waterBody.fill)} FillTile:{Math.Round(waterBody.GetFillTile(),2)}";  
            else
                text = "Incorrect Water Body ID";
            ChatManager.instance.Print(text);
        }));
        commandList.Add(new DebugCommand("help", "command list", "", () =>
        {
            string text = "Command list:\n";
            for (int i = 0; i < commandList.Count; i++)
            {
                CommandBase commandBase = commandList[i] as CommandBase;
                text += $"/{commandBase.commandId} {commandBase.commandFormat} - {commandBase.commandDescription}\n";
            }
            ChatManager.instance.Print(text);
        })); 
        return commandList;
    }

}
