
using UnityEngine;

public class GameInfo : MonoBehaviour 
{
    public bool multiplayerMode;
    public int nextScene;
    public static GameInfo Instance { get; private set; }

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

