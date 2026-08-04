using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;



public class GridObjectAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject sprite;
    [SerializeField] private GameObject shadow;

    public class Baker : Baker<GridObjectAuthoring>
    {
        public override void Bake(GridObjectAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnvironmentObject>(entity); 
            AddComponent<ClientGridObject>(entity, new ClientGridObject()
            {
                sprite = GetEntity(authoring.sprite,TransformUsageFlags.Dynamic),
                shadow = GetEntity(authoring.shadow,TransformUsageFlags.Dynamic),
            });
        }
    }
}

