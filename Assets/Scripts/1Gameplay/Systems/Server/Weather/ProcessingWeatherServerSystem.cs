using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(WeatherServerSystem))]
public partial struct ProcessingWeatherServerSystem : ISystem
{
    private uint period;
    private NetworkTick previousUpdate;
    private NativeHashSet<Entity> processed;
    private ComponentLookup<LocalWeather> weatherLookup;
    private ComponentLookup<PlayerSourceConnection> connectionsLookup;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<MapSettings>(); 
        state.RequireForUpdate<LocalWeather>(); 

        period = (uint)(NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate * WorldConfig.WeatherConfig.ProcessingWeatherUpdatePeriod);
        processed = new NativeHashSet<Entity>(16,Allocator.Persistent);
        weatherLookup = SystemAPI.GetComponentLookup<LocalWeather>(true);
        connectionsLookup = SystemAPI.GetComponentLookup<PlayerSourceConnection>(true);
    }
    public void OnDestroy()
    {
        processed.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        if(!previousUpdate.IsValid || currentTick.TicksSince(previousUpdate) >= period)
        {
            weatherLookup.Update(ref state);
            connectionsLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var mapSettings = SystemAPI.GetSingleton<MapSettings>();
            var currentTime = SystemAPI.GetSingleton<CurrentTime>();

            foreach (var(ChunkComponent,ContainsPlayers,playersNeedChunk, chunkObjects, chunkEntity) in SystemAPI
                .Query<RefRO<ChunkComponent>,EnabledRefRO<ContainsPlayers>,DynamicBuffer<PlayersNeedChunk>,DynamicBuffer<ChunkObjects>>()
                .WithEntityAccess())
            {
                float storm = 0;
                int count = 0;

                foreach(var item in playersNeedChunk)
                {
                    if(processed.Contains(item.playerEntity))
                        continue;

                    storm += weatherLookup[item.playerEntity].Storm;
                    count ++;
                    processed.Add(item.playerEntity);
                }

                if(count > 0 && WeatherService.GetLightning(mapSettings.seed,currentTime.WorldTime,ChunkComponent.ValueRO.worldPos,((float)storm)/count,currentTick,in mapSettings,out float2 lightningPosition))
                {
                    Debug.Log("ligthing  " + lightningPosition);
                    RPCHelper.SendEventsToClientsImmediately(new LightningRPC(){ position = lightningPosition },ecb,connectionsLookup,playersNeedChunk,chunkEntity);
                }
            }
            processed.Clear();
            previousUpdate = currentTick;
            ecb.Playback(state.EntityManager);
        }
    }
}

