
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

    public string playerName;
    public string worldName;
    public long creationTime;
    public Difficulty difficultyLevel;
    public double playTime;
    public int seed;
    [Space]
    public bool isMultiplayer;
    public bool isHost;
    [Space]
    public int loadingMode;
    public int nextScene;
    public float maxProgress;
    public int lastLoadedScene;
    [Space]
    public string errorMessage;



    public static GameInfo instance { get; private set; }


    public void SetDefaultSettings()
    {
        difficultyLevel = Difficulty.Normal;
        worldName = string.Empty;
        playTime = 0;
        Debug.Log("co???");
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

    public void SetValue(HeaderData data)
    {
        playerName = data.playerName.ToString();
        Debug.Log(data.playerName); 
        worldName = data.worldName;
        creationTime = data.creationTime;
        difficultyLevel = data.difficulty;
        playTime = data.playTime;
        seed = data.seed;
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

