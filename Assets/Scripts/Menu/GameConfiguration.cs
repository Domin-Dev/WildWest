
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

public class GameConfiguration: MonoBehaviour 
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Animator blackScreen;

    private GameObject gameObj;
    private void Start()
    {
        GameInfo.SetUp();
        gameObj = Instantiate(loadingScreen);
        LoadingManager loading = gameObj.GetComponent<LoadingManager>();
        loading.SetStartValue(0.5f);
        loading.gameIsReady += () =>
        {
            Destroy(gameObj);
            blackScreen.SetTrigger("BlackScreen");
        };
        ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity(typeof(LoadMap));
    }
}

