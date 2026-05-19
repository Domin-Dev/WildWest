using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;


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
        var playersList = SystemAPI.GetSingletonBuffer<PlayersList>(false);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;


        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, GoInGameRequestRPC requestRPC, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, GoInGameRequestRPC>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(rpcCommandRequest.ValueRO.SourceConnection);
            var networkId = state.EntityManager.GetComponentData<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;

            Entity character = entityCommandBuffer.Instantiate(SystemAPI.GetSingleton<EntitiesReferences>().characterEntity);

            if (!SaveIOThread.TryLoadPlayer(requestRPC.playerName.ToString(),out PlayerSave playerSave, out var containers))
                GetDefaultPlayerSave(requestRPC, ref playerSave);

            entityCommandBuffer.AddComponent(rpcCommandRequest.ValueRO.SourceConnection,new LinkedCharacter() { entity = character }); 
            entityCommandBuffer.SetComponent(character, LocalTransform.FromPosition(new float3(playerSave.playerPosition.x, playerSave.playerPosition.y, playerSave.playerPosition.y)));
            entityCommandBuffer.SetComponent(character, new PlayerLook() { look = playerSave.characterLook });
            entityCommandBuffer.AddComponent(character, new GhostOwner { NetworkId = networkId }); 
            entityCommandBuffer.SetComponent(character, new Player()
            {
                speed = 1.7f,
                playerName = playerSave.playerName
            });
            playersList.Add(new PlayersList()
            {
               networkID = networkId,
               playerName = playerSave.playerName
            });


            entityCommandBuffer.AddComponent(character, new ServerChunkEventCounter() { index = uint.MaxValue });

            if (SystemAPI.HasComponent<Host>(rpcCommandRequest.ValueRO.SourceConnection) ||
                playerSave.isAdmin)
                entityCommandBuffer.AddComponent<Admin>(rpcCommandRequest.ValueRO.SourceConnection);


            entityCommandBuffer.AddComponent(character, new GhostChunk().StartValues());
            entityCommandBuffer.AddComponent(character, new NewChunk());
            entityCommandBuffer.SetComponentEnabled<NewChunk>(character, true);
            entityCommandBuffer.AddComponent(character, new SendToOwner());
            entityCommandBuffer.AddBuffer<GhostChildren>(character);


            AddMapComponents(ref entityCommandBuffer,character);
            AddEquipmentEntities(ref state, ref entityCommandBuffer, character, rpcCommandRequest.ValueRO.SourceConnection, networkId,containers);



            entityCommandBuffer.AddBuffer<FutureEventsForPlayer>(character);
            entityCommandBuffer.SetComponent<Health>(character, new Health() { Max = 100, Value = playerSave.health });
            entityCommandBuffer.SetComponent<Hunger>(character, new Hunger() { Max = 100, Value = playerSave.hunger });
            entityCommandBuffer.SetComponent<Thirst>(character, new Thirst() { Max = 100, Value = playerSave.thirst });    
            entityCommandBuffer.AddComponent<ToSave>(character);
            entityCommandBuffer.SetComponentEnabled<ToSave>(character,true);

            entityCommandBuffer.AppendToBuffer(rpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup() { Value = character });

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
        entityCommandBuffer.AddComponent(character, new GhostChunk());
        entityCommandBuffer.AddComponent(character, new NewChunk());
        entityCommandBuffer.SetComponentEnabled<NewChunk>(character, false);
        var key = new RelevantGhostForConnection()
        {
            Ghost = SystemAPI.GetComponent<GhostInstance>(character).ghostId,
            Connection = networkID
        };
        SystemAPI.GetSingletonRW<GhostRelevancy>().ValueRW.GhostRelevancySet.TryAdd(key, 0);
    }

    private void GetDefaultPlayerSave(GoInGameRequestRPC requestRPC,ref PlayerSave playerSave)
    {
        playerSave = new PlayerSave()
        {
            playerName = requestRPC.playerName,
            characterLook = requestRPC.characterLook,
            playerPosition = float2.zero,
            health = 100,
            thirst = 100,
            hunger = 100,
            isAdmin = false
        };
    }

    private void AddEquipmentEntities(ref SystemState state, ref EntityCommandBuffer ecb, Entity character,Entity connection, int networkID, ContainerSave[] containerSaves)
    {
        var entities = SystemAPI.GetSingleton<EntitiesReferences>();
        ecb.AddComponent<ContainerSettings>(character, new ContainerSettings() {
            Position = SlotPosition.NullSlot,
            targetContainer = -1,
            nextTempIndex = EquipmentConfig.start_ContainerIndex
        });

        var containers = EquipmentConfig.Instance.Containers;
        foreach(var cont in containers)
        { 
            ContainerSave? containerSave = null;
            if(containerSaves != null)
            {
                foreach(var save in containerSaves)
                {
                    if(save.containerIndex == cont.Value.stats.containerIndex)
                        containerSave = save;
                }
            }
            CreateNewContainer(ref state,character, connection,ecb,ref entities,networkID,cont.Value.stats,containerSave);
        }

        if(containerSaves != null)
        {
            foreach(var i in containerSaves)
                i.Dispose();
        }
    }
    private void AddMapComponents(ref EntityCommandBuffer ecb, Entity character)
    {
        ecb.AddBuffer<PlayerChunks>(character);
    }
    public static Entity CreateNewContainer(ref SystemState state,Entity player,EntityCommandBuffer entityCommandBuffer,ref EntitiesReferences entities,ContainerStats stats,ContainerSave? containerSave = null, int parentContainerIndex = -1)
    {
        Entity connection = state.EntityManager.GetComponentData<PlayerSourceConnection>(player).value;
        int networkID = state.EntityManager.GetComponentData<NetworkId>(connection).Value;
        return CreateNewContainer(ref state, player,connection,entityCommandBuffer,ref entities,networkID,stats,containerSave,parentContainerIndex);
    }
  
    [BurstCompile]
    public static Entity CreateNewContainer(Entity chunk, EntityCommandBuffer.ParallelWriter entityCommandBuffer,int chunkIndex,ref EntitiesReferences entities,ContainerStats stats,ContainerSave? containerSave = null,int parentContainerIndex = -1, int sortKey = 0)
    {
        var e = entityCommandBuffer.Instantiate(sortKey,entities.equipmentContainerEntity);
        entityCommandBuffer.SetComponent(sortKey,e, new ContainerComponent() {
            containerStats = stats,
            parentContainerIndex = parentContainerIndex
        });
        if(stats.serverContainer)
           entityCommandBuffer.AddComponent<ServerContainer>(sortKey,e);

        entityCommandBuffer.SetComponent(sortKey,e,new ContainerOwner(){ owner = chunk});
        entityCommandBuffer.AddComponent(sortKey,e, new ServerEquipmentEventCounter() { index = uint.MaxValue });
        entityCommandBuffer.AppendToBuffer<EntityContainers>(sortKey,chunk, new EntityContainers() { entity = e, index = stats.containerIndex});   
        entityCommandBuffer.SetBuffer<InventorySlot>(sortKey,e).EnsureCapacity(stats.capacity + 1);
        entityCommandBuffer.SetBuffer<ItemBarData>(sortKey,e).EnsureCapacity(stats.capacity + 1);
        entityCommandBuffer.AppendToBuffer(sortKey,chunk, new LinkedEntityGroup() { Value = e });

        if(containerSave != null)
        {
            for(int i = 0; i < containerSave.Value.slots.Length;i++)
            {
                SlotSave slot = containerSave.Value.slots[i];
                if(slot.itemId >= 0)
                {
                    entityCommandBuffer.AppendToBuffer(sortKey,e,new InventorySlot(slot,i));
                    BarDataSave barDataSave = containerSave.Value.barData[i];
                    if(barDataSave.value >= 0)
                    {
                        entityCommandBuffer.AppendToBuffer(sortKey,e,new ItemBarData(barDataSave,i));
                    }
                }
            }
        }
        entityCommandBuffer.AddComponent<GhostChunk>(sortKey,e,new GhostChunk(){current = chunkIndex});
        entityCommandBuffer.AddComponent<NewChunk>(sortKey,e);
        return e;
    }
    public static Entity CreateNewContainer(ref SystemState state,Entity player,Entity connection, EntityCommandBuffer entityCommandBuffer,ref EntitiesReferences entities,int networkID,ContainerStats stats,ContainerSave? containerSave = null,int parentContainerIndex = -1)
    {
        var e = state.EntityManager.Instantiate(entities.equipmentContainerEntity);
        entityCommandBuffer.AddComponent(e, new GhostOwner() { NetworkId = networkID });
        entityCommandBuffer.SetComponent(e, new ContainerComponent() {
            containerStats = stats,
            parentContainerIndex = parentContainerIndex
        });
        if(stats.serverContainer)
            entityCommandBuffer.AddComponent<ServerContainer>(e);

        entityCommandBuffer.AddComponent(e, new ServerEquipmentEventCounter() { index = uint.MaxValue });
        entityCommandBuffer.AppendToBuffer<EntityContainers>(player, new EntityContainers() { entity = e, index = stats.containerIndex});   
        entityCommandBuffer.SetBuffer<InventorySlot>(e).EnsureCapacity(stats.capacity + 1);
        entityCommandBuffer.SetBuffer<ItemBarData>(e).EnsureCapacity(stats.capacity + 1);
        entityCommandBuffer.AppendToBuffer(connection, new LinkedEntityGroup() { Value = e });
        entityCommandBuffer.AddComponent(e,new ContainerOwner(){ owner = player});


        if(containerSave != null)
        {
            for(int i = 0; i < containerSave.Value.slots.Length;i++)
            {
                SlotSave slot = containerSave.Value.slots[i];
                if(slot.itemId >= 0)
                {
                    entityCommandBuffer.AppendToBuffer(e,new InventorySlot(slot,i));
                    BarDataSave barDataSave = containerSave.Value.barData[i];
                    if(barDataSave.value >= 0)
                    {
                        entityCommandBuffer.AppendToBuffer(e,new ItemBarData(barDataSave,i));
                    }
                }
            }
        }
        return e;
    }

}
