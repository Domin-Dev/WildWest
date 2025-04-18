using System;
using System.Net;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEditor.Build.Pipeline;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;

    [SerializeField] private GameObject blackBackground;
    [SerializeField] private GameObject connectionWindow;
    [Space]
    [SerializeField] private Button buttonSingleplayer;
    [SerializeField] private Button buttonMultiplayer;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonQuit;
    [Space]
    [SerializeField] private Button buttonConnet;
    [Space]
    [SerializeField] private TMP_InputField adressIPInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Dropdown connectionMode;
    [Space]
    [SerializeField] private TextMeshProUGUI errorMessage;

    private void Awake()
    {
        SetUpUI();
        errorMessage.gameObject.SetActive(false);
    }

    private async void OnEnable()
    {
        buttonMultiplayer.onClick.AddListener(OpenMultiplayerWindow);
        buttonConnet.onClick.AddListener(OnButtonConnect);
        buttonQuit.onClick.AddListener(Quit);

        adressIPInput.onValueChanged.AddListener((x) => { if (CheckIP(x)) ErrorTurnOff();});
        portInput.onValueChanged.AddListener((x) => { if (CheckPORT(x)) ErrorTurnOff(); });
    }

    private void OnDisable()
    {
        buttonMultiplayer.onClick.RemoveAllListeners();
        buttonConnet.onClick.RemoveAllListeners();
        buttonQuit.onClick.RemoveAllListeners();


        adressIPInput.onValueChanged.RemoveAllListeners();

    }

    private void SetUpUI()
    {
        versionText.text = Application.productName + " " + Application.version;
    }

    private void OpenMultiplayerWindow()
    {
        blackBackground.SetActive(true);
        connectionWindow.SetActive(true);
    }

    private static readonly Regex ipv4Regex = new Regex(
            @"^((25[0-5]|2[0-4]\d|[01]?\d?\d)\.){3}"
          + @"(25[0-5]|2[0-4]\d|[01]?\d?\d)$",
            RegexOptions.Compiled
        );

    private void PrintError(string value)
    {
        errorMessage.gameObject.SetActive(true);
        buttonConnet.interactable = false;
        errorMessage.text = value;
    }
    private void ErrorTurnOff()
    {
        if (!buttonConnet.interactable)
        {
            if (CheckIP(adressIPInput.text) && CheckPORT(portInput.text))
            {
                errorMessage.gameObject.SetActive(false);
                buttonConnet.interactable = true;
            }
        }
    }

    private bool CheckIP(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            PrintError("No connection address.");
            return false;
        }
        if(!ipv4Regex.IsMatch(value))
        {
            PrintError("Incorrect connection address.");
            return false;
        }
        return true;
    }
    private bool CheckPORT(string value)
    {
        if (int.TryParse(value, out int port))
        {
            if (port >= 1 && port <= 65535)
            {
                return true;
            }
            else
            {
                PrintError("Port number out of range.");
                return false;
            }
        }
        PrintError("Invalid port format.");
        return false;
    }
    private void CloseMultiplayerWindow()
    {
        blackBackground.SetActive(false);
        connectionWindow.SetActive(false);
    }

 
    
    
    
    private void Quit()
    {
        Application.Quit();
    }
    private void OnButtonConnect()
    {
        DestroyLocalSimulationWorld();
        SceneManager.LoadScene(1);

        switch (connectionMode.value)
        {
            case 0:
                StartClient();
                break;
            case 1:
                StartServer();
                StartClient();
                break;
            case 2:
                StartServer();
                break;
            default:
                Debug.LogError("Error: Unknown connection mode", gameObject);
                break;
        }
    }
    private static void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            Debug.Log(world.Name + " " + world.Flags);
            if (world.Flags == WorldFlags.GameClient)
            {
                world.Dispose();
                break;
            }
        }
    }
    private void StartServer()
    {
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Turbo Server World");

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(ushort.Parse(portInput.text));
        {
            using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }
    }
    private void StartClient()
    {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Turbo Client World");

        var connectionEndpoint = NetworkEndpoint.Parse(adressIPInput.text,ushort.Parse(portInput.text));
        {
            using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        }

        World.DefaultGameObjectInjectionWorld = clientWorld;


        Entity entity = clientWorld.EntityManager.CreateEntity();
        clientWorld.EntityManager.AddComponentData(entity, new PlayerName() {name = playerNameInput.text.ToString()});
    }
}
