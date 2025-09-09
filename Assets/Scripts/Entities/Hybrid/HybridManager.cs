using Cinemachine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class HybridManager : MonoBehaviour
{
    [SerializeField] GameObject trailBullet;
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    public static HybridManager instance;

    private Dictionary<Entity,GameObject> connectedObjects = new Dictionary<Entity,GameObject>();    


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            IsConnetedCilientSystem.youAreInGame += IsPlayer;
        }
    }

    private void OnDestroy()
    {
        IsConnetedCilientSystem.youAreInGame -= IsPlayer;
    }

    private void IsPlayer()
    {
        EntityFollower entityFollower = new GameObject("PlayerFollower", typeof(EntityFollower)).GetComponent<EntityFollower>();
        entityFollower.transform.position = new Vector3(0, 0, -10f);

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<Player,GhostOwnerIsLocal>();
        var entites = entityQueryBuilder.Build(ClientServerBootstrap.ClientWorld.EntityManager);
        var array = entites.ToEntityArray(Allocator.Temp);
        entityFollower.SetEntity(array[0],false);

        var pos = ClientServerBootstrap.ClientWorld.EntityManager.GetComponentData<LocalTransform>(array[0]);
        Vector3 cameraPos = virtualCamera.transform.position;
        cameraPos.x = pos.Position.x;
        cameraPos.y = pos.Position.y;

        Debug.Log(pos);

        virtualCamera.transform.position = cameraPos;
        Camera.main.transform.position = cameraPos;
        virtualCamera.Follow = entityFollower.transform;
        entites.Dispose();
        entityQueryBuilder.Dispose();
        array.Dispose();
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
}
