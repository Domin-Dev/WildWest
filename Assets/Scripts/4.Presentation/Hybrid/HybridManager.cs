using Cinemachine;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HybridManager : MonoBehaviour
{
    [SerializeField] GameObject trailBullet;
    [SerializeField] private GameObject gameObjectPlayer;
    public Transform PlayerCamera {private set; get;}
    public Transform GameObjectPlayer => gameObjectPlayer.transform;


    [SerializeField] CinemachineVirtualCamera virtualCamera;

    public static HybridManager instance;
    private Dictionary<Entity,GameObject> connectedObjects = new Dictionary<Entity,GameObject>(); 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameObjectPlayer.transform.position = Vector3.zero;
        PlayerCamera = new GameObject("PlayerCamera").transform;
        PlayerCamera.transform.position = new Vector3(0, 0, -10f);
        virtualCamera.Follow = PlayerCamera.transform;
    }

    public void SetEntity(Entity entity,Vector3 position)
    {
        GameObject obj = Instantiate(trailBullet, position, Quaternion.identity);
        obj.GetComponent<EntityFollower>().SetEntity(entity,true);
        connectedObjects.Add(entity, obj);
    }
    public void EntityDeleted(Entity entity)
    {
        if (connectedObjects.ContainsKey(entity))
        {
            GameObject obj = connectedObjects[entity];
            Destroy(obj);
            connectedObjects.Remove(entity);
        }
    }

    public Transform GetLight(LinkedLight linkedLight)
    {
        var obj = new GameObject("Light",typeof(Light2D)).GetComponent<Light2D>();
        obj.intensity = linkedLight.Intensity;
        obj.pointLightInnerRadius = linkedLight.InnerRadius;
        obj.pointLightOuterRadius = linkedLight.OuterRadius;
        obj.falloffIntensity = linkedLight.FalloffIntensity;
        return obj.transform;
    }

}
    