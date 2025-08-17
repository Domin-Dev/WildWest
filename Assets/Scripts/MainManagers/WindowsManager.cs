using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class WindowsManager : MonoBehaviour
{
    public int escScene = -1;

    [SerializeField] private GameObject blackBackgroundPrefab;
    private GameObject blackBackground;

    private List<int> loadedScene = new List<int>();
    public static WindowsManager instance { private set; get; }

    public event Action OnCloseWindows;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            blackBackground = Instantiate(blackBackgroundPrefab);
            SwitchBackground(false);
            DontDestroyOnLoad(blackBackground);
            loadedScene = new List<int>();  
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (escScene == -1)
            {
                CloseOpenWindows();
            }
        }
    }



    public void LoadScene(int index)
    {
        bool load = !loadedScene.Contains(index);
        LoadScene(index, load);
    }
    public void LoadScene(int index, bool background)
    {
        bool load = !loadedScene.Contains(index);
        if (load)
        {
            if (!SceneManager.GetSceneByBuildIndex(index).isLoaded)
                SceneManager.LoadScene(index, LoadSceneMode.Additive);
            CloseOpenWindows();
            loadedScene.Add(index);
        }
        else
        {
            UnloadScene(index);
        }
        SwitchBackground(background);
    }
    public bool CloseOpenWindows()
    {
        OnCloseWindows?.Invoke();
        SwitchBackground(false);

        if (loadedScene.Count == 0) return false;
        for (int i = 0; i < loadedScene.Count; i++)
        {
            UnloadScene(loadedScene[i]);
        }
        loadedScene.Clear();
        return true;
    }
    public void UnloadScene(int index)
    {
        if (SceneManager.GetSceneByBuildIndex(index).isLoaded)
            SceneManager.UnloadSceneAsync(index);
        loadedScene.Remove(index);
    }
    public void SwitchBackground(bool value)
    {
        blackBackground.SetActive(value);
    }
}

