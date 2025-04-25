
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.NetCode;
using Unity.Networking.Transport;
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

    private void Start()
    {
        SetUpUI();
        errorMessage.gameObject.SetActive(false);

        buttonMultiplayer.onClick.AddListener(OpenMultiplayerWindow);
        buttonSingleplayer.onClick.AddListener(OnButtonCreateGame);

        buttonConnet.onClick.AddListener(OnButtonConnect);
        buttonQuit.onClick.AddListener(Quit);

        adressIPInput.onValueChanged.AddListener((x) => { if (CheckIP(x)) ErrorTurnOff();});
        portInput.onValueChanged.AddListener((x) => { if (CheckPORT(x)) ErrorTurnOff(); });
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
        GameInfo.Instance.isConnecting = true;
        GameInfo.Instance.nextScene = 2;
        SceneManager.LoadScene(3);
       
        switch (connectionMode.value)
        {
            case 0:
                Join();
                break;
            case 1:
                RunServer();
                break;
            case 2:
                //RunServer();
                //Join();
                break;
            default:
                Debug.LogError("Error: Unknown connection mode", gameObject);
                break;
        }
    }
    private void OnButtonCreateGame()
    {
        GameInfo.Instance.isConnecting = false;
        GameInfo.Instance.nextScene = 2;
        SceneManager.LoadScene(3);


    }
    private void Join()
    {
        for (int i = World.All.Count - 1; i >= 0; i--)
        {
            World world = World.All[i];
            if (world.Flags == WorldFlags.GameClient || world.Flags == WorldFlags.GameServer)
            {
                World.All[i].Dispose();
            }
        }

        World clientWorld = ClientServerBootstrap.CreateClientWorld("Client Wild world");

        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        ushort port = ushort.Parse(portInput.text);
        string ip = adressIPInput.text;

        NetworkEndpoint networkEndpoint = NetworkEndpoint.Parse(ip, port);
        RefRW<NetworkStreamDriver> networkStreamDriver =
            clientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, networkEndpoint);


        Entity entity = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity();
        ClientServerBootstrap.ClientWorld.EntityManager.AddComponentData(entity, new PlayerName() { name = playerNameInput.text.ToString() });
        Debug.Log("Próba po³¹czenia");
        ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity(typeof(EnableConnectionTimeoutCheck));
    }

    private void RunServer()
    {
        foreach (World world in World.All)
        {
            if (world.Flags == WorldFlags.GameClient)
            {
                world.Dispose();
                break;
            }
        }

        World serverWorld = ClientServerBootstrap.CreateServerWorld("Server wild world");

        World clientWorld = ClientServerBootstrap.CreateClientWorld("Client Wild world");
        


        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }

     //   SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);

        ushort port = ushort.Parse(portInput.text);


        RefRW<NetworkStreamDriver> networkStreamDriver =
            serverWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));

        NetworkEndpoint networkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
        networkStreamDriver =
            clientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, networkEndpoint);



        Entity entity = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity();
        ClientServerBootstrap.ClientWorld.EntityManager.AddComponentData(entity, new PlayerName() { name = playerNameInput.text.ToString() });
        Debug.Log("Próba po³¹czenia");
        ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity(typeof(EnableConnectionTimeoutCheck));
    }
   
}
