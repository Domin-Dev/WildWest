
using System;
using Unity.Collections;
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
    [HideInInspector]public bool startGame;

    public string playerName;
    public string worldName;
    public FixedString128Bytes? passHash;   
    public int playerLimit;
    public long creationTime;
    public Difficulty difficultyLevel;
    public double playTime;
    public int seed;
    [Space]
    public bool isMultiplayer;
    public bool isHost;
    public bool isInGame;
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

    public void SetValue(HeaderSave data)
    {
        playerName = data.playerName.ToString();
        worldName = data.worldName.ToString();
        creationTime = data.creationTime;
        difficultyLevel = data.difficulty;
        playTime = data.playTime;
        seed = data.seed;
    }

    public HeaderSave GetHeader()
    {
        HeaderSave headerData = new HeaderSave();
        headerData.playerName = playerName;
        headerData.difficulty = difficultyLevel;
        headerData.playTime = playTime;
        headerData.worldName = worldName; 
        headerData.seed = seed;   
        headerData.creationTime = creationTime;
        return headerData;
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

