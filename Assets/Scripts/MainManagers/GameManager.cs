using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public event EventHandler<ValueArgs> onValueChange;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var scene = SceneManager.GetSceneByBuildIndex(1);
            if(!scene.isLoaded)
            {
                SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.UnloadSceneAsync(1);
            }
        }
    }
    private void OnValue(object s, EventArgs e)
    {
        Debug.Log("dziala");
    }
}

