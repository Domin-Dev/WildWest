


// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.NetCode;
// using Unity.Physics;

// [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
// public partial struct PlayerMovementSystem : ISystem
// {

//     public void OnCreate(ref SystemState state)
//     {
        
//     }

//     public void OnUpdate(ref SystemState state)
//     {    

//         foreach (var (input, velocity,player)
//                  in SystemAPI.Query<
//                      RefRO<PlayerInput>,
//                      RefRW<PhysicsVelocity>,RefRO<Player>>())
//         {

//             velocity.ValueRW.Linear.x =  input.ValueRO.movementDirection.x * player.ValueRO.speed;
//             velocity.ValueRW.Linear.y =  input.ValueRO.movementDirection.y * player.ValueRO.speed;
//         }
//     }

// }