using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;



// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [UpdateAfter(typeof(ClientTimeSystem))]
// public partial struct ServerWeatherSystem : ISystem
// {
//     public static Action<LocalWeather,LocalWeather,CurrentTime> OnWeatherUpdate;
//     private int period;
//     private NetworkTick lastTick;
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<CurrentTime>();
//         state.RequireForUpdate<TimeConfig>();
//         state.RequireForUpdate<NetworkTime>();
//         state.RequireForUpdate<LocalWeather>();
//         state.RequireForUpdate<MapSettings>();
//         period = (int)(NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate * 60 * WorldConfig.TimeConfig.HourDuration * WorldConfig.WeatherConfig.WeatherUpdatePeriod);
        
//         if(SystemAPI.HasSingleton<LocalWeather>())
//         {
//             var time = SystemAPI.GetSingleton<NetworkTime>();
//             SystemAPI.GetSingletonRW<LocalWeather>().ValueRW = time.ServerTick;
//         }
//     }
//     public void OnUpdate(ref SystemState state)
//     {
//         var networkTime = SystemAPI.GetSingleton<NetworkTime>();
//         NetworkTick currentTick = networkTime.ServerTick;
//         if(currentTick.IsValid && (!lastTick.IsValid || currentTick.TicksSince(lastTick) >= period))
//         {
//             var localWeather = SystemAPI.GetSingletonRW<LocalWeather>();
//             var mapSettings = SystemAPI.GetSingleton<MapSettings>();
//             var currentTime = SystemAPI.GetSingleton<CurrentTime>();
//             float2 playerPosition = float2.zero;    

//             foreach (RefRO<LocalToWorld> position in  SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<GhostOwnerIsLocal,Player>().WithNone<NewPlayerTag>())
//             {
//                 playerPosition = new float2(position.ValueRO.Position.x,position.ValueRO.Position.y);
//             }
            
//             LocalWeather previousValue = localWeather.ValueRO;
//             localWeather.ValueRW = WeatherService.GetWeather(mapSettings.seed,currentTime,playerPosition);
//             OnWeatherUpdate?.Invoke(previousValue,localWeather.ValueRO,currentTime);
//             lastTick.Add((uint)period);
//         }
//     }
// }

