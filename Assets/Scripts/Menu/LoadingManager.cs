
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingWindow;
    [SerializeField] private GameObject errorWindow;
    [SerializeField] private GameObject password;
    [Space]
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button errorButton;
    [Space]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image loadingBar;
    [Space]
    [SerializeField] private Button sendPassword;
    [SerializeField] private TMP_InputField inputField;

    private float target;
    private FixedString128Bytes salt;



    public Action gameIsReady;


    private void Awake()
    {
        errorButton.onClick.AddListener(()=> { SceneManager.LoadScene(0);});
    }

    private void Start()
    {
        loadingWindow.SetActive(true);
        errorWindow.SetActive(false);

        switch (GameInfo.instance.loadingMode)
        {
            case 0:
                Connecting();
                break;
            case 1:
                Loading(GameInfo.instance.maxProgress);
                break;
            case 2:
                LoadGame();
                break;
        }
    }

    private void OnDestroy()
    {
        ConnectionTimeoutSystem.connectionFailed -= ConnectionFailed;
        ConnectionTimeoutSystem.connectionSuccessful -= Connected;
        WaitForConfirmation.connection -= Verification;
        ClientAuthSystem.passwordRequired -= PasswordRequired;
    }

    private void Connecting()
    {
        loadingText.text = "Connecting...";
        ConnectionTimeoutSystem.connectionSuccessful += Connected;
        ConnectionTimeoutSystem.connectionFailed += ConnectionFailed;
        ClientAuthSystem.passwordRequired += PasswordRequired; 
        WaitForConfirmation.connection += Verification;
    }
    private async void Connected()
    {
        target = 0.5f;
        await Task.Delay(200);
        SetValue(0.5f);
        ConnectionTimeoutSystem.connectionSuccessful -= Connected;
        loadingText.text = "Verifying...";
    }
    private async void Verification(CharacterLook? characterLook)
    {
        if (characterLook.HasValue)
        {
            Debug.Log("new Look ");
            GameInfo.instance.nextScene = 1;
            LocalPlayerLook playerLook = new LocalPlayerLook();
            playerLook.characterLook = characterLook.Value;
            Entity e = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity();
            ClientServerBootstrap.ClientWorld.EntityManager.AddComponentData(e, playerLook);
        }

        await Task.Delay(50);
        loadingText.text = "Loading...";
        Loading(1f, 0.5f);
        WaitForConfirmation.connection -= Verification;
    }
    private void LoadGame()
    {
        loadingText.text = "Configuring...";
        IsConnetedCilientSystem.youAreInGame += StartGame;
    }
    public void SetStartValue(float value)
    {
        target = value;
        SetValue(value);
    }
    private async void StartGame()
    {
        target = 1f;
        await Task.Delay(200);
        SetValue(1f);        
        await Task.Delay(100);
        IsConnetedCilientSystem.youAreInGame -= StartGame;
        gameIsReady?.Invoke();
    }
    private void ConnectionFailed()
    {
        ConnectionTimeoutSystem.connectionFailed -= ConnectionFailed;
        PrintError("Connection failed");
    }
    private void Loading(float maxProgress, float startProgress = 0f)
    {
        loadingText.text = "Loading...";
        LoadAsyncScene(GameInfo.instance.nextScene, maxProgress, startProgress);
    }
    private void SetValue(float value)
    {
        if(loadingBar != null) loadingBar.rectTransform.anchorMax = new Vector2(math.clamp(value,0f,1f), 1);
    } 
    private async void LoadAsyncScene(int index, float maxProgress = 1f, float startProgress = 0f)
    {
        var operation = SceneManager.LoadSceneAsync(index,LoadSceneMode.Additive);
        operation.allowSceneActivation = false;
        do
        {
            await Task.Delay(20);
            target = (startProgress + Mathf.Clamp01(operation.progress / 0.9f) * (1 -startProgress)) * maxProgress;
        }
        while (operation.progress < 0.9f);

        await Task.Delay(200);
        SetValue(maxProgress);
        target = maxProgress;

        await Task.Delay(50);
        operation.allowSceneActivation = true;
        await operation;
        await SceneManager.UnloadSceneAsync(3);
    }
    private void Update()
    {
        float lerp = math.lerp(loadingBar.rectTransform.anchorMax.x, target, Time.deltaTime * 7f);
        SetValue(lerp);
    }
    public void PrintError(string message)
    {
        errorWindow.SetActive(true);
        loadingWindow.SetActive(false);
        errorText.text = message;
    }
    public void PasswordRequired(FixedString128Bytes salt)
    {
        this.salt = salt;
        loadingWindow.SetActive(false);
        password.SetActive(true);
        sendPassword.onClick.AddListener(SendPassword);
        inputField.text = "";
    }


    private void SendPassword()
    {
        loadingWindow.SetActive(true);
        password.SetActive(false);
        sendPassword.onClick.RemoveAllListeners();

        Debug.Log(inputField.text + " " + salt);
        var hash = AuthUtils.ComputeSha256(inputField.text.ToArray());
        inputField.text = "";
        var finishHash = AuthUtils.GetSaltHash(hash,salt);
        Entity entity = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity(typeof(SendRpcCommandRequest));
        ClientServerBootstrap.ClientWorld.EntityManager.AddComponentData(entity, new ClientHashRPC() { hash = finishHash });
    }
}
