
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;


    [SerializeField] private GameObject connectionWindow;
    [SerializeField] private GameObject worldListWindow;
    [SerializeField] private GameObject multiplayerOptionsWindow;
    [SerializeField] private GameObject confirmationRemoveWindow;
    [SerializeField] private GameObject editWorldWindow;
    [SerializeField] private GameObject connectToIPWindow;
    [SerializeField] private GameObject serverSettingsWindow;

    [Header("Server Settings")]
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Switch playerLimit;
    [SerializeField] private Button buttonHostServer;
    [SerializeField] private Button buttonBackSettingsServer;

    [Header("Connect To IP")]
    [SerializeField] private TMP_InputField adressIPInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button buttonConnet;
    [SerializeField] private Button buttonBackConnectToIP;
    [SerializeField] private TextMeshProUGUI errorMessage;

    [Header("Edit")]
    [SerializeField] private WorldNameInput worldNameInput;
    [SerializeField] private Button editYes;
    [SerializeField] private Button editNo;

    [Header("Confirmation")]
    [SerializeField] private LocalizeStringEvent confirmationText;
    [SerializeField] private Button confirmationYes;
    [SerializeField] private Button confirmationNo;

    [Header("Multiplayer Options")]
    [SerializeField] private Button buttonBack;
    [SerializeField] private Button buttonConnectToIP;
    [SerializeField] private Button buttonHostGame;

    [Header("Worlds")]
    [SerializeField] private Button buttonBackWorlds;
    [SerializeField] private Button buttonNewWorld;
    [SerializeField] private GameObject worldList;
    [SerializeField] private GameObject worldRow;
    [SerializeField] private GamepadScroll gamepadDropdownScroll;

    [Header("Connection")]
    [SerializeField] private CanvasGroup menuButtons;
    [SerializeField] private Button buttonSingleplayer;
    [SerializeField] private Button buttonMultiplayer;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonQuit;

    [Space]
    [SerializeField] private TMP_Dropdown connectionMode;

    public static MenuManager instance;

    private string worldName;
    bool isSingleplayerList;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void Start()
    {
        GameInfo.instance.lastLoadedScene = -1;
        SetUpUI();
        errorMessage.gameObject.SetActive(false);

        GameInfo.instance.SetDefaultSettings();
        WindowsManager.instance.OnCloseWindows += CloseWindows;
    }

    private void OnEnable()
    {

        /////////////////////////////////////////
        buttonMultiplayer.onClick.AddListener(() => {
            SwitchMenuButtons(false);
            OpenWindow(multiplayerOptionsWindow, buttonBack);
        });
        buttonSingleplayer.onClick.AddListener(() =>
        {
            isSingleplayerList = true;
            OpenWorldList();
        });
        buttonSettings.onClick.AddListener(() =>
        {
            GameInfo.instance.lastLoadedScene = -1;
            WindowsManager.instance.LoadScene(8);
            SwitchMenuButtons(false);
        });
        buttonQuit.onClick.AddListener(Quit);
        /////////////////////////////////////////
        buttonBack.onClick.AddListener(CloseWindows);
        buttonHostGame.onClick.AddListener(() =>
        {
            isSingleplayerList = false;
            OpenWorldList();
        });
        buttonConnectToIP.onClick.AddListener(() =>
        {
            CloseWindows();
            OpenWindow(connectToIPWindow, buttonBackConnectToIP);
        });
        /////////////////////////////////////////
        buttonBackWorlds.onClick.AddListener(CloseWindows);
        /////////////////////////////////////////
        confirmationYes.onClick.AddListener(() =>
        {
            Debug.Log("usowanie!!  " + worldName);
            WorldManager.RemoveWorld(worldName);
            OpenWorldList();
        });
        confirmationNo.onClick.AddListener(OpenWorldList);
        //////////////////////////////////////////
        editNo.onClick.AddListener(OpenWorldList);
        editYes.onClick.AddListener(() =>
        {
            string name = worldNameInput.GetWorldName();
            if (name != string.Empty)
            {
                if (WorldManager.ChangeName(worldName, name))
                {
                    OpenWorldList();
                }
            }
        });
        //////////////////////////////////////////
        buttonConnet.onClick.AddListener(() => Starter.Join(playerNameInput.text,adressIPInput.text,ushort.Parse(portInput.text)));
        buttonBackConnectToIP.onClick.AddListener(CloseWindows);
        adressIPInput.onValueChanged.AddListener((x) => { if (CheckIP(x)) ErrorTurnOff(); });
        portInput.onValueChanged.AddListener((x) => { if (CheckPORT(x)) ErrorTurnOff(); });
        //////////////////////////////////////////
        buttonBackSettingsServer.onClick.AddListener(CloseWindows);
        //////////////////////////////////////////
    }
    private void OnDisable()
    {
     /////////////////////////////////////////
        buttonMultiplayer.onClick.RemoveAllListeners();
        buttonSingleplayer.onClick.RemoveAllListeners();
        buttonSettings.onClick.RemoveAllListeners();
        buttonQuit.onClick.RemoveAllListeners();
        /////////////////////////////////////////
        buttonBack.onClick.RemoveAllListeners();
        buttonHostGame.onClick.RemoveAllListeners();
        buttonConnectToIP.onClick.RemoveAllListeners();
        /////////////////////////////////////////
        buttonBackWorlds.onClick.RemoveAllListeners();
        /////////////////////////////////////////
        confirmationYes.onClick.RemoveAllListeners();
        confirmationNo.onClick.RemoveAllListeners();
        //////////////////////////////////////////
        editNo.onClick.RemoveAllListeners();
        editYes.onClick.RemoveAllListeners();
        //////////////////////////////////////////
        buttonConnet.onClick.RemoveAllListeners();
        buttonBackConnectToIP.onClick.RemoveAllListeners();
        adressIPInput.onValueChanged.RemoveAllListeners();
        portInput.onValueChanged.RemoveAllListeners();
        //////////////////////////////////////////
        buttonBackSettingsServer.onClick.RemoveAllListeners();
        //////////////////////////////////////////
    }
    private void OnDestroy()
    {
        WindowsManager.instance.OnCloseWindows -= CloseWindows;
    }
    private void SetUpUI()
    {
        versionText.text = Application.productName + " " + Application.version;
        SetSelectedButton();
    }


    public void SetSelectedButton()
    {
       WindowsManager.instance.SetNewSelectedButton(buttonSingleplayer?.gameObject);
    }
    public void SwitchMenuButtons(bool turnON)
    {
       if(menuButtons!= null) menuButtons.interactable = turnON;
    }
    public void Confirmation(string worldName)
    {
        CloseWindows();
        OpenWindow(confirmationRemoveWindow, confirmationNo);
        confirmationText.StringReference.Arguments = new object[] { $"<Color=#5b3138>{worldName}</Color>"};
        confirmationText.RefreshString();
        this.worldName = worldName;
    }
    public void Edit(string worldName)
    {
        CloseWindows();
        OpenWindow(editWorldWindow,editNo);
        worldNameInput.SetUp(worldName);
        this.worldName = worldName;
    }
    public void Load(string worldName)
    {
        if (!SaveIOThread.TryLoadHeader(worldName,out HeaderSave data)) return;

        CloseWindows();
        if (isSingleplayerList)
            Starter.LoadSingleplayerWorld(data);
        else
            LoadMultiplayer(data);
    }

    private void LoadMultiplayer(HeaderSave data)
    {
        OpenServerSettings();
        buttonHostServer.onClick.RemoveAllListeners();
        buttonHostServer.onClick.AddListener(() => RunServer(data));
    }


    private void OpenWindow(GameObject window, Button selectedButton)
    {
        SwitchMenuButtons(false);
        WindowsManager.instance.SwitchBackground(true);
        WindowsManager.instance.SetNewSelectedButton(selectedButton.gameObject);
        window.SetActive(true);
    }

    private void OpenServerSettings()
    {
        CloseWindows();
        OpenWindow(serverSettingsWindow, buttonBackSettingsServer);
        passwordInput.text = string.Empty;
        playerLimit.SetUpSwitch(1, 17,string.Empty);
    }
    private void OpenWorldList()
    {
        CloseWindows();
        if(worldList.transform.childCount > 0)
        {
            for (int i = worldList.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(worldList.transform.GetChild(i).gameObject);
            }
        }

        buttonNewWorld.onClick.RemoveAllListeners();

        
        if(isSingleplayerList)
            buttonNewWorld.onClick.AddListener(OpenWorldSetUp);
        else
            buttonNewWorld.onClick.AddListener(OpenServerSettings);

        List<(HeaderSave header, PlayerSave playerData)> headers = LoadSystem.LoadHeaders()?.OrderByDescending(s => s.header.saveTime).ToList();

        if (headers != null)
        {
            foreach (var header in headers)
            {
                GameObject gameObject = Instantiate(worldRow, worldList.transform);
                WorldRow row = gameObject.GetComponent<WorldRow>();
                row.SetWorld(header.header,header.playerData, UIAssetsManager.instance.UIHeadMaterial);
            }
        }

        OpenWindow(worldListWindow, buttonBackWorlds);
        gamepadDropdownScroll.RefreshButtons();
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
    private void CloseWindows()
    {
        SwitchMenuButtons(true);
        SetSelectedButton();
        WindowsManager.instance.SwitchBackground(false);
        connectionWindow?.SetActive(false);
        worldListWindow?.SetActive(false);
        multiplayerOptionsWindow?.SetActive(false);
        confirmationRemoveWindow?.SetActive(false);
        editWorldWindow?.SetActive(false);
        connectToIPWindow?.SetActive(false);
        serverSettingsWindow?.SetActive(false);
    }
    private void Quit()
    {
        Application.Quit();
    }
    private void OnButtonCreateGame()
    {
        GameInfo.instance.isMultiplayer = false;
        GameInfo.LoadScene(2, 1);
    }

    public void OpenWorldSetUp()
    {
        Debug.Log("setip!!!!");
        GameInfo.LoadScene(4, 1);
    }

    public void CreateoWrld()
    {
        GameInfo.LoadScene(4, 0);
        GameInfo.instance.playerName = playerNameInput.text.ToString();
        GameInfo.instance.passHash = string.IsNullOrEmpty(passwordInput.text) ? AuthUtils.ComputeSha256(passwordInput.text.ToArray()) : null;
        GameInfo.instance.playerLimit = playerLimit.GetValue();
    }

    
    public void RunServer(HeaderSave? headerData = null)
    {
        GameInfo.instance.isMultiplayer = true;
        GameInfo.instance.isHost = true;
        WindowsManager.instance.SwitchBackground(false);
        
        if(headerData == null)
        {
            GameInfo.LoadScene(4, 0);
            GameInfo.instance.playerName = playerNameInput.text.ToString();
            GameInfo.instance.passHash = string.IsNullOrEmpty(passwordInput.text) ? AuthUtils.ComputeSha256(passwordInput.text.ToArray()) : null;
            GameInfo.instance.playerLimit = playerLimit.GetValue();
        }
        else
        {
            GameInfo.instance.SetValue(headerData.Value);
            GameInfo.LoadScene(1, 0, 0.5f);
        }

        for (int i = World.All.Count - 1; i >= 0; i--)
        {
            World world = World.All[i];
            if(world.Flags == WorldFlags.GameClient || world.Flags == WorldFlags.GameServer)
            {
                world.Dispose();
            }
        }


        Debug.Log("Start!!!");

        GameInfo.instance.startGame = true;
        World serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWildWorld");
        World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWildWorld");
        GameInfo.instance.startGame = false;

        ClientWorldSetUp(clientWorld);

        if (World.DefaultGameObjectInjectionWorld == null)
            World.DefaultGameObjectInjectionWorld = serverWorld;
        

        ushort port = ushort.Parse(portInput.text);

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



    // private void ServerWorldSetUp()
    // {
    //     Entity entity = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity();
    //     ClientServerBootstrap.ClientWorld.EntityManager.AddComponentData(entity, new PlayerName() { name = GameInfo.instance.playerName });
    //     ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity(typeof(EnableConnectionTimeoutCheck));

    //     bool isPassword  = GameInfo.instance.passHash.HasValue;
        
    //     ClientServerBootstrap.ServerWorld.EntityManager.CreateSingleton(new ServerData()
    //     {
    //         hash = GameInfo.instance.passHash.Value,
    //         isPassword = isPassword,
    //         isHost = true,
    //         playersLimit = GameInfo.instance.playerLimit,
    //         hostNetworkID = int.MinValue,
    //     });   

    //     ClientServerBootstrap.ServerWorld.EntityManager.CreateSingleton(new MapSettings()
    //     {
    //         seed = GameInfo.instance.seed,
    //         widthInChunks = 10,
    //         heightInChunks = 10,
    //         playerRenderSize = 2,
    //         maxChunksPerClient = 30,
    //         maxLoadedChunksInTick = 40,
    //         loadedChunksInTickPerClient =  5
    //     });     
    //     ClientServerBootstrap.ServerWorld.EntityManager.CreateSingletonBuffer<LoadedChunks>();
    // }

    private void ClientWorldSetUp(World clientWorld)
    {
        var simGroup = clientWorld.GetExistingSystemManaged<SimulationSystemGroup>(); 
        var mapLoadingSystem = clientWorld.GetOrCreateSystemManaged<MapLoadingClientSystem>();

        simGroup.AddSystemToUpdateList(mapLoadingSystem);
        simGroup.SortSystems();
    }
}
