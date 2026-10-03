using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


public class WindowsManager : MonoBehaviour
{
    public int escScene = -1;

    [SerializeField] private GameObject blackBackgroundPrefab;
    private GameObject blackBackground;

    private List<int> loadedScene = new List<int>();
    public static WindowsManager instance { private set; get; }
    public event Action OnCloseWindows;
    private GameObject selectedObject;



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

    void OnLevelWasLoaded(int level)
    {
        SwitchBackground(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !ChatManager.instance.isChatting) 
        {
            if (escScene == -1)
                CloseOpenWindows();
            else
            {
                if (!CloseOpenWindows())
                    LoadScene(escScene);
            }
        }
    }


    public void SetNewSelectedButton(GameObject gObject)
    {
        selectedObject = gObject;
        if(!DisableMouseInputSystem.mouseEnabled) 
            EventSystem.current.SetSelectedGameObject(gObject);
    }
    public void RefreshSelection()
    {
        if (selectedObject != null)
            EventSystem.current.SetSelectedGameObject(selectedObject);
    }
    public void ClearSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }


    public bool HasOpenWidnows()
    {
        return loadedScene.Count > 0;
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
        blackBackground?.SetActive(value);
    }
}

