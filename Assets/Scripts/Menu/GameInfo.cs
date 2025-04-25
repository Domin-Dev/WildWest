
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInfo : MonoBehaviour 
{
    public bool isConnecting;
    public int nextScene;
    public static GameInfo Instance { get; private set; }


    public static void LoadScene(int newScene)
    {
        GameInfo.Instance.isConnecting = false;
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

