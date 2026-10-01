using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    public class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Hands());
            AddComponent(entity, new Character());
            AddComponent(entity, new Player() { speed = 1f });
            AddComponent(entity, new NewPlayerTag());
            AddComponent(entity, new PlayerInput()
            {
                sightDirection = new float2(float.MinValue,float.MinValue),
                slotInHand = 0
            });
            AddComponent(entity, new PlayerInputSync()
            {
                slotInHand = -1
            });
            AddComponent(entity, new PlayerLook());

            AddComponent(entity, new PlayerActionSpread());
            AddComponent(entity, new ShootingSkill());




            AddComponent(entity, new AimRotation());
            AddComponent(entity, new Cooldown());
            AddComponent(entity, new CurrentPlayerState(){ state = PlayerState.none});
            
            AddComponent(entity, new GhostChunk().StartValues());

 
            AddComponent(entity, new Health());
            AddComponent(entity, new Hunger());
            AddComponent(entity, new Thirst());


            AddBuffer<DamageBuffer>(entity);
            AddBuffer<EntityContainers>(entity);
        }
    }
}
