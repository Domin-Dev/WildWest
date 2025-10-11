
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;


public class Dashboard : MonoBehaviour
{
    public const float updateInterval = 3f;


    [SerializeField] private GameObject dashboard;
    [SerializeField] private GameObject rowClient;
    [SerializeField] private GameObject rowAdmin;

    World world;


    public static Dashboard instance;

    List<PlayerData> players;
    Dictionary<int,Row> pingDashborad;
    int count;
    int max;

    private void Awake()
    {
        instance = this;
        world = MyTools.GetClientWorld();
        players = new List<PlayerData>();
        pingDashborad = new Dictionary<int,Row>();
        count = 0;
        max = 0;
    }

    private void Start()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
        RPCHelper.SendRpc<GetPlayerDashboardRPC>(ref entityCommandBuffer);
        entityCommandBuffer.Playback(world.EntityManager);
        entityCommandBuffer.Dispose();
        StartCoroutine(UpdateStats());
    }

    public void AddPlayers(in PlayerList playerList)
    {
        max = playerList.max;
        count++;
        for (int i = 0; i < 6; i++)
        {
            if (playerList[i].playerName.IsEmpty) break;
            players.Add(playerList[i]);
        }
        if (count == max) BuildDashboard();
    }

    public void UpdatePing(in PingList pingList)
    {
        for (int i = 0; i < pingList.length; i++)
        {
            var ping = pingList[i];
            if(pingDashborad.ContainsKey(ping.playerID))
                pingDashborad[ping.playerID].pingText.text = ping.ping + " ms";
        }
        Debug.Log("update ping! " + pingList.length + " " + pingList[0].ping);
    }


    Material mat;
    private void BuildDashboard()
    {
        players.Sort((a, b) => a.playerID.CompareTo(b.playerID));
        GameObject rowPrefab = rowClient;

        if (ClientServerBootstrap.HasServerWorld)
            rowPrefab = rowAdmin;

        foreach (PlayerData playerData in players)
        {
            GameObject row = Instantiate(rowPrefab, dashboard.transform);
            Row rowComp = row.GetComponent<Row>();

            rowComp.playerName.text = playerData.playerName.ToString();
            mat = new Material(UIAssetsManager.instance.UIHeadMaterial);
            rowComp.playerIcon.material = mat;
            rowComp.pingText.text = "???";

            mat.SetColor("_SkinColor", MyTools.GetColorFromFloat3(playerData.characterLook.skinColor));
            mat.SetColor("_HairColor", MyTools.GetColorFromFloat3(playerData.characterLook.hairColor));
            mat.SetInt("_HairIndex", playerData.characterLook.hairIndex);
            mat.SetInt("_BeardIndex", playerData.characterLook.beardndex);
            mat.SetInt("_PaintingsIndex", playerData.characterLook.faceDetailsIndex);

            rowComp.playerIcon.SetMaterialDirty();
            pingDashborad.Add(playerData.playerID, rowComp);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(UpdateStats());   
    }

    IEnumerator UpdateStats()
    {
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            UpdatePing();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdatePing()
    {
        Entity entity = world.EntityManager.CreateEntity();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
        RPCHelper.SendRpc<GetPingRPC>(ref entityCommandBuffer);
        entityCommandBuffer.Playback(world.EntityManager);
        entityCommandBuffer.Dispose();
        Debug.Log("zapytanie!");
    }

}

