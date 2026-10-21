using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using EditorAttributes;
using System;


[RequireComponent(typeof(Configs))]
public class AppBoot : MonoBehaviour
{
    private void Awake()
    {
        
    }
    private void Start()
    {
        if(TryGetComponent<Configs>(out var configs))
        {
            Debug.Log("AppBoot");
#if UNITY_EDITOR 
            configs.CheckConfigs();
#endif
            foreach (ConfigCore config in configs.GetConfigs())
                if (config != null) config.SetUp();
            SceneManager.LoadScene(Scenes.Menu);
        }
        else
            Debug.LogError("Missing configurations");
        Destroy(this);
    }
}