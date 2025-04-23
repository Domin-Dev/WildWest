
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image loadingBar;

    private float target;


    private void Start()
    {
        if (GameInfo.Instance.multiplayerMode)
            MultiplayerMode();
        else
            SingleplayerMode();
    }

    private void MultiplayerMode()
    {
        loadingText.text = "Connecting...";
        LoadAsyncScene(GameInfo.Instance.nextScene);

    }

    private void SingleplayerMode()
    {
        loadingText.text = "Loading...";
        LoadAsyncScene(GameInfo.Instance.nextScene);
    }

    private void SetValue(float value)
    {
        loadingBar.rectTransform.anchorMax = new Vector2(value, 1);
    }
    
    private async void LoadAsyncScene(int index)
    {
        var operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = false;

        do
        {
            await Task.Delay(50);
            target = Mathf.Clamp01(operation.progress / 0.9f);
        }
        while (operation.progress < 0.9f);

        await Task.Delay(1000);

        SetValue(1f);
        operation.allowSceneActivation = true;
    }

    private void Update()
    {
        float lerp = math.lerp(loadingBar.rectTransform.anchorMax.x, target, Time.deltaTime * 4f);
        SetValue(lerp);
    }

}
