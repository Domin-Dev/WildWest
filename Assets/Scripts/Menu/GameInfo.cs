
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInfo : MonoBehaviour 
{
    public int loadingMode;
    public int nextScene;
    public static GameInfo Instance { get; private set; }


    public static void LoadScene(int newScene, int loadingMode)
    {
        GameInfo.Instance.loadingMode = loadingMode;
        GameInfo.Instance.nextScene = newScene;
        SceneManager.LoadScene(3);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}

