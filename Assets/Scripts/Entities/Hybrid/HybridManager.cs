using Cinemachine;
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
        Debug.Log("------------------------------------------------jes tpal");
        EntityFollower entityFollower = new GameObject("PlayerFollower", typeof(EntityFollower)).GetComponent<EntityFollower>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<Player,GhostOwnerIsLocal>();
        var entites = entityQueryBuilder.Build(ClientServerBootstrap.ClientWorld.EntityManager);
        var array = entites.ToEntityArray(Allocator.Temp);
        Debug.Log("Array ma " + array.Length);  
        entityFollower.SetEntity(array[0]);


        virtualCamera.Follow = entityFollower.transform;
        entites.Dispose();
        entityQueryBuilder.Dispose();
        array.Dispose();
    }

    public void SetEntity(Entity entity,Vector3 position)
    {
        Instantiate(trailBullet,position,Quaternion.identity).GetComponent<EntityFollower>().SetEntity(entity);
    }
}
