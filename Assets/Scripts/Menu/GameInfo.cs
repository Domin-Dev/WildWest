
using UnityEngine;
using UnityEngine.SceneManagement;




public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class GameInfo : MonoBehaviour 
{
    public string worldName;
    public long creationTime;
    public Difficulty difficultyLevel;
    public double playTime;

    public int seed;
    public bool isMultiplayer;
    public bool isHost;
    //////////////////
    public int loadingMode;
    public int nextScene;
    public float maxProgress;
    //////////////////
    public string errorMessage;
    //////////////////
    public int lastLoadedScene;
    //////////////////


    public static GameInfo instance { get; private set; }


    public void SetDefaultSettings()
    {
        difficultyLevel = Difficulty.Normal;
        playTime = 0;

    }
    public static void LoadScene(int newScene, int loadingMode, float maxProgress = 1f)
    {
        instance.loadingMode = loadingMode;
        instance.nextScene = newScene;
        instance.maxProgress = maxProgress;
        SceneManager.LoadScene(3);
    }
    public static void SetUp()
    {
        instance.loadingMode = 2;
        instance.nextScene = 1;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}

