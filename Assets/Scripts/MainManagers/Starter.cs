
using System;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Starter
{
    public static void RunServer(HeaderData? headerData = null, ushort port = 7979)
    {
        GameInfo.instance.isMultiplayer = true;
        GameInfo.instance.isHost = true;
        WindowsManager.instance.SwitchBackground(false);
        
        if(headerData != null) 
            GameInfo.instance.SetValue(headerData.Value);
        GameInfo.LoadScene(1, 0, 0.5f);
    

        for (int i = World.All.Count - 1; i >= 0; i--)
        {
            World world = World.All[i];
            if(world.Flags == WorldFlags.GameClient || world.Flags == WorldFlags.GameServer)
            {
                world.Dispose();
            }
        }


        GameInfo.instance.startGame = true;
        World serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWildWorld");
        World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWildWorld");
        GameInfo.instance.startGame = false;

        ClientWorldSetUp(clientWorld);

        if (World.DefaultGameObjectInjectionWorld == null)
            World.DefaultGameObjectInjectionWorld = serverWorld;
        

        RefRW<NetworkStreamDriver> networkStreamDriver =
        serverWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();

 
        try
        {
            var endPoint = NetworkEndpoint.AnyIpv4.WithPort(port);
            if (!endPoint.IsValid) throw new Exception($"Invalid endpoint: port {port} is out of range or address is invalid.");
            networkStreamDriver.ValueRW.RequireConnectionApproval = true;
            bool result = networkStreamDriver.ValueRW.Listen(endPoint);

            if (!result) throw new Exception($"Failed to listen on port {endPoint.Port}. Port may be in use.");
         
            NetworkEndpoint networkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
            networkStreamDriver =
                clientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
            networkStreamDriver.ValueRW.RequireConnectionApproval = true;    
            networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, networkEndpoint);
        }
        catch
        (Exception ex)
        {

            GameInfo.instance.errorMessage = ex.Message;    
            SceneManager.LoadScene(10);
        }
    }


    public static void CreateSinglePlayerWorld()
    {
        GameInfo.instance.playerName = "Player";
        RunServer();
    }

    private static void ClientWorldSetUp(World clientWorld)
    {
        var simGroup = clientWorld.GetExistingSystemManaged<SimulationSystemGroup>(); 
        var mapLoadingSystem = clientWorld.GetOrCreateSystemManaged<MapLoadingClientSystem>();

        simGroup.AddSystemToUpdateList(mapLoadingSystem);
        simGroup.SortSystems();
    }
}