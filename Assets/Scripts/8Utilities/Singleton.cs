using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { private set; get; }
    protected virtual void Awake()
    {
        if(Instance == null)
            Instance = (T)this;
        else
            Destroy(gameObject);  
    }
}