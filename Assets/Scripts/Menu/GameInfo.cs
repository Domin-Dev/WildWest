
using UnityEngine;
using UnityEngine.SceneManagement;




public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class GameInfo : MonoBehaviour 
{


    public string worldName;
    public Difficulty difficultyLevel;
    public int seed;
    //////////////////
    public int loadingMode;
    public int nextScene;
    public float maxProgress;
    public static GameInfo instance { get; private set; }


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

