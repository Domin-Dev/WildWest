
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.VisualScripting;
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


    private void Awake()
    {
        errorButton.onClick.AddListener(()=> { SceneManager.LoadScene(0);});
    }

    private void Start()
    {
        loadingWindow.SetActive(true);
        errorWindow.SetActive(false);

        switch (GameInfo.Instance.loadingMode)
        {
            case 0:
                Connecting();
                break;
            case 1:
                Loading(0);
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
    private void Connected()
    {
        target = 0.5f;
        loadingText.text = "Loading...";
        ConnectionTimeoutSystem.connectionSuccessful -= Connected;
        Loading(0.5f);
    }
    private void LoadGame()
    {
        Load(GameInfo.Instance.nextScene,0.5f);
        IsConnetedCilientSystem.youAreInGame += StartGame;
    }

    private void StartGame()
    {
        Debug.Log("dzialKO!!!");
        target = 1f;
        SetValue(1f);
        Unload(3);
        IsConnetedCilientSystem.youAreInGame -= StartGame;
    }
    private void ConnectionFailed()
    {
        ConnectionTimeoutSystem.connectionFailed -= ConnectionFailed;
        PrintError("Connection failed");
    }
    private void Loading(float progress)
    {
        loadingText.text = "Loading...";
        LoadAsyncScene(GameInfo.Instance.nextScene, progress);
    }
    private void SetValue(float value)
    {
        loadingBar.rectTransform.anchorMax = new Vector2(value, 1);
    } 
    private async void LoadAsyncScene(int index, float progress = 0f)
    {
        Debug.Log("wczytywanie");
        var operation = SceneManager.LoadSceneAsync(index,LoadSceneMode.Additive);
        operation.allowSceneActivation = false;

        do
        {
            await Task.Delay(20);
            target = progress +  Mathf.Clamp01(operation.progress / 0.9f) * (1 - progress);
        }
        while (operation.progress < 0.9f);

        await Task.Delay(200);

        SetValue(1f);
        operation.allowSceneActivation = true;
        await operation;
        await SceneManager.UnloadSceneAsync(3);
    }
    private async void Load(int index, float maxProgress = 1f)
    {
        Debug.Log("wczytywanie");
        var operation = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;
        do
        {
            await Task.Delay(20);
            target =  Mathf.Clamp01(operation.progress / 0.9f) * maxProgress;
        }
        while (operation.progress < 0.9f);

        await Task.Delay(200);

        operation.allowSceneActivation = true;
        await operation;
    }
    private async void Unload(int index)
    {
        var operation = SceneManager.UnloadSceneAsync(index);
        await operation;
    }


    private void Update()
    {
        Debug.Log(target);
        float lerp = math.lerp(loadingBar.rectTransform.anchorMax.x, target, Time.deltaTime * 6f);
        SetValue(lerp);
    }
    private void PrintError(string message)
    {
        errorWindow.SetActive(true);
        loadingWindow.SetActive(false);
        errorText.text = message;
    }

}
