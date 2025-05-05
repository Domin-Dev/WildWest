using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class HybridManager : MonoBehaviour
{
    [SerializeField] GameObject trailBullet;

    [SerializeField] Transform k;
    public static HybridManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void SetEntity(Entity entity,Vector3 position)
    {
        Instantiate(trailBullet,position,Quaternion.identity).GetComponent<EntityTrailFollower>().SetEntity(entity);
    }
}
