
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingWindow;
    [SerializeField] private GameObject errorWindow;
    [Space]
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button errorButton;
    [Space]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image loadingBar;

    private float target;

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
    }

    private void Connecting()
    {
        loadingText.text = "Connecting...";
        ConnectionTimeoutSystem.connectionSuccessful += Connected;
        ConnectionTimeoutSystem.connectionFailed += ConnectionFailed;
    }
    private async void Connected()
    {
        target = 0.5f;
        await Task.Delay(200);

        SetValue(0.5f);
        await Task.Delay(50);

        loadingText.text = "Loading...";
        ConnectionTimeoutSystem.connectionSuccessful -= Connected;
        Loading(1f,0.5f);
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

}
