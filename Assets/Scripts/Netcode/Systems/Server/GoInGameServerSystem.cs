using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.TextCore.Text;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequestRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, GoInGameRequestRPC requestRPC, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, GoInGameRequestRPC>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(rpcCommandRequest.ValueRO.SourceConnection);
            var networkId = state.EntityManager.GetComponentData<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;

            Entity character = entityCommandBuffer.Instantiate(SystemAPI.GetSingleton<EntitiesReferences>().characterEntity);
            PlayerSave playerSave = LoadSystem.LoadPlayerSave(requestRPC.playerName.ToString());

            if (playerSave == null)
                GetDefaultPlayerSave(requestRPC, ref playerSave);


            entityCommandBuffer.AddComponent(rpcCommandRequest.ValueRO.SourceConnection,new LinkedCharacter() { entity = character }); 
            entityCommandBuffer.SetComponent(character, LocalTransform.FromPosition(new float3(playerSave.playerPosition.x, playerSave.playerPosition.y, playerSave.playerPosition.y)));
            entityCommandBuffer.SetComponent(character, new PlayerLook() { look = playerSave.characterLook });
            entityCommandBuffer.AddComponent(character, new GhostOwner { NetworkId = networkId });
            entityCommandBuffer.AddComponent(character, new InterestArea() { radius = 8f });
            entityCommandBuffer.SetComponent(character, new Player()
            {
                speed = 1f,
                playerName = playerSave.playerName
            });



            entityCommandBuffer.AddComponent(character, new ServerChunkEventCounter() { index = uint.MaxValue });

            if (SystemAPI.HasComponent<Host>(rpcCommandRequest.ValueRO.SourceConnection) ||
                playerSave.isAdmin)
                entityCommandBuffer.AddComponent<Admin>(rpcCommandRequest.ValueRO.SourceConnection);


            entityCommandBuffer.AddComponent(character, new LastChunk());
            entityCommandBuffer.AddComponent(character, new NeedChunks());
            entityCommandBuffer.SetComponentEnabled<NeedChunks>(character, true);
            entityCommandBuffer.AddComponent(character, new SendToPlayer());


            AddEquipmentEntities(ref state, ref entityCommandBuffer, character, networkId);
            

            entityCommandBuffer.SetComponent<Health>(character, new Health() { Max = 100, Value = playerSave.health });
            entityCommandBuffer.SetComponent<Hunger>(character, new Hunger() { Max = 100, Value = playerSave.hunger });
            entityCommandBuffer.SetComponent<Thirst>(character, new Thirst() { Max = 100, Value = playerSave.thirst });

            //entityCommandBuffer.AppendToBuffer(rpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup() { Value = character });

            Entity confirmation = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent<YouAreInGameRPC>(confirmation);
            entityCommandBuffer.AddComponent(confirmation, new SendRpcCommandRequest() { TargetConnection = rpcCommandRequest.ValueRO.SourceConnection });
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void ServerComponents(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, Entity character,int networkID)
    {
        entityCommandBuffer.AddComponent(character, new LastChunk());
        entityCommandBuffer.AddComponent(character, new NeedChunks());
        entityCommandBuffer.SetComponentEnabled<NeedChunks>(character, false);
        var key = new RelevantGhostForConnection()
        {
            Ghost = SystemAPI.GetComponent<GhostInstance>(character).ghostId,
            Connection = networkID
        };
        SystemAPI.GetSingletonRW<GhostRelevancy>().ValueRW.GhostRelevancySet.TryAdd(key, 0);
    }

    private void GetDefaultPlayerSave(GoInGameRequestRPC requestRPC,ref PlayerSave playerSave)
    {
        playerSave = new PlayerSave();
        playerSave.playerName = requestRPC.playerName;
        playerSave.characterLook = requestRPC.characterLook;
        playerSave.playerPosition = float2.zero;
        playerSave.health = 100;
        playerSave.thirst = 100;
        playerSave.hunger = 100;
        playerSave.isAdmin = false;
    }

    private void AddEquipmentEntities(ref SystemState state, ref EntityCommandBuffer ecb, Entity character, int networkID)
    {
        var entities = SystemAPI.GetSingleton<EntitiesReferences>();
        ecb.AddBuffer<PlayerContainers>(character);
        ecb.AddComponent<ContainerSettings>(character, new ContainerSettings() {
            Position = SlotPosition.NullSlot,
            targetContainer = -1
        });

        var containers = EquipmentConfig.Instance.Containers;
        foreach(var cont in containers)
        { 
            CreateNewContainer(character,ref ecb,ref entities,networkID,cont.Value);
        }

        



        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0,networkID,10,0);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,50,networkID,30,1);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,100, networkID, 20, 2, MandatoryProperties.item, 30);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 8, 3, MandatoryProperties.item, 34);


        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10000, MandatoryProperties.tag, 0);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10001, MandatoryProperties.tag, 1);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10002, MandatoryProperties.tag, 2);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10003, MandatoryProperties.tag, 3);

        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10004, MandatoryProperties.tag, 4);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10005, MandatoryProperties.tag, 5);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10006, MandatoryProperties.tag, 6);
        // CreateNewContainer(character, ref entityCommandBuffer, ref entities,0, networkID, 1, 10007, MandatoryProperties.tag, 7);

    }
    private void CreateNewContainer(Entity player,ref EntityCommandBuffer entityCommandBuffer,ref EntitiesReferences entities,int networkID,byte waterResistance, int capacity, int index, MandatoryProperties mandatory = MandatoryProperties.none, int mandatoryData = -1)
    {
        var e = entityCommandBuffer.Instantiate(entities.equipmentContainerEntity);
        entityCommandBuffer.AddComponent(e, new GhostOwner() { NetworkId = networkID });
        entityCommandBuffer.SetComponent(e, new ContainerComponent() {
            containerStats = new ContainerStats()
            { 
                capacity = capacity,
                containerIndex = index,
                mandatoryProperties = mandatory,
                mandatoryData = mandatoryData,
                waterResistance = waterResistance
            }
        });

        entityCommandBuffer.AddComponent(e, new GhostChildEntity());
        entityCommandBuffer.AppendToBuffer<GhostGroup>(player, new GhostGroup() { Value = e });

        entityCommandBuffer.AddComponent(e, new ServerEquipmentEventCounter() { index = uint.MaxValue });
        entityCommandBuffer.AppendToBuffer<PlayerContainers>(player, new PlayerContainers() { entity = e, index = index});
        entityCommandBuffer.SetBuffer<InventorySlot>(e).EnsureCapacity(capacity + 1);


        entityCommandBuffer.AddComponent(e, new SendToPlayer());
    }


    private void CreateNewContainer(Entity player,ref EntityCommandBuffer entityCommandBuffer,ref EntitiesReferences entities,int networkID, ContainerData data)
    {
        CreateNewContainer(player,ref entityCommandBuffer,ref entities,networkID,data.stats.waterResistance,data.stats.capacity,
        data.stats.containerIndex,data.stats.mandatoryProperties,data.stats.mandatoryData);
    }
}
