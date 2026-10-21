

using UnityEngine;
public abstract class ConfigCore : ScriptableObject
{
    public abstract void SetUp();
} 

public abstract class Config<T> : ConfigCore where T : Config<T>
{   
    public static T Instance {private set;get;}
    public static void SetInstance(T config)
    {
        if (config == null) 
        { 
            Debug.LogError("SetInstance: null");
            return; 
        }
        Instance = config;
    }
    public sealed override void SetUp()
    {
        SetInstance((T)this);
        Start();
    }
    protected virtual void Start(){}
} 