using Unity.Entities;
using UnityEngine;

public class AnimationAuthoring : MonoBehaviour
{
    public BodyPartType bodyPartType;
    public class Baker : Baker<AnimationAuthoring>
    {
        public override void Bake(AnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Transform parent = authoring.transform.parent;
            while(parent != null && parent.tag != "Player")
            {
                parent = parent.parent;
            }
            Entity playerEntitiy = GetEntity(parent.gameObject,TransformUsageFlags.Dynamic);


            AddComponent(entity, new AnimationComponent() 
            {
                 bodyPartType = authoring.bodyPartType,
                 elapsedTime = 0,
                 player = playerEntitiy
            });
            AddBuffer<AnimationFrames>(entity);
            AddBuffer<AnimationEvents>(entity);


            AddComponent<AnimationPaused>(entity);
            SetComponentEnabled<AnimationPaused>(entity,true);
        }
    }
}
