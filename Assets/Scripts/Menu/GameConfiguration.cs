
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
    private LoadingManager loading;
    private void Start()
    {
        GameInfo.SetUp();
        gameObj = Instantiate(loadingScreen);
        loading = gameObj.GetComponent<LoadingManager>();
        loading.SetStartValue(0.5f);
        loading.gameIsReady += Action;

        ClientServerBootstrap.ClientWorld.GetExistingSystemManaged<MapLoadingClientSystem>().SetUp();
        ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity(typeof(LoadMap));

    }

    public void Action()
    {
        Destroy(gameObj);
        if(blackScreen != null) blackScreen.SetTrigger("BlackScreen");
        WindowsManager.instance.escScene = 11;
        loading.gameIsReady -= Action;
    }
}

