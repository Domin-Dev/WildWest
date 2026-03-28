using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public event EventHandler<ValueArgs> onValueChange;
    const int DebugUIIndex = 5;
    public class ValueArgs : EventArgs
    {
        public int x;
        public int y;
        public ValueArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public static GameManager instance { private set; get; }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void OnEnable()
    {
        Debug.Log("need key!");
        if(InputManager.i != null)  InputManager.i.debugStats.performed += OpenDebugStats;
    }
    private void OnDisable()
    {
        if (InputManager.i != null) InputManager.i.debugStats.performed -= OpenDebugStats;
    }
    private void OpenDebugStats(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var scene = SceneManager.GetSceneByBuildIndex(DebugUIIndex);
        if (!scene.isLoaded)
            SceneManager.LoadSceneAsync(DebugUIIndex, LoadSceneMode.Additive);
        else
            SceneManager.UnloadSceneAsync(DebugUIIndex);
    }
    private void OnValue(object s, EventArgs e)
    {
        Debug.Log("dziala");
    }
}

