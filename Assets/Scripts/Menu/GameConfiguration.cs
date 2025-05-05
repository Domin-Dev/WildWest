
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameConfiguration: MonoBehaviour 
{
    [SerializeField] private GameObject loadingScreen;

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
        };
    }

}

