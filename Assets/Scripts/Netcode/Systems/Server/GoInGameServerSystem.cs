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
                speed = 1f,
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
 


            AddMapComponents(ref entityCommandBuffer,character);
            AddEquipmentEntities(ref state, ref entityCommandBuffer, character, networkId,containers);
            
            entityCommandBuffer.SetComponent<Health>(character, new Health() { Max = 100, Value = playerSave.health });
            entityCommandBuffer.SetComponent<Hunger>(character, new Hunger() { Max = 100, Value = playerSave.hunger });
            entityCommandBuffer.SetComponent<Thirst>(character, new Thirst() { Max = 100, Value = playerSave.thirst });    
            entityCommandBuffer.AddComponent<ToSave>(character);
            entityCommandBuffer.SetComponentEnabled<ToSave>(character,true);

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

    private void AddEquipmentEntities(ref SystemState state, ref EntityCommandBuffer ecb, Entity character, int networkID, ContainerSave[] containerSaves)
    {
        var entities = SystemAPI.GetSingleton<EntitiesReferences>();
        ecb.AddComponent<ContainerSettings>(character, new ContainerSettings() {
            Position = SlotPosition.NullSlot,
            targetContainer = -1
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
            CreateNewContainer(character,ref ecb,ref entities,networkID,cont.Value,containerSave);
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
    
    private void CreateNewContainer(Entity player,ref EntityCommandBuffer entityCommandBuffer,ref EntitiesReferences entities,int networkID,byte waterResistance, int capacity, int index, MandatoryProperties mandatory = MandatoryProperties.none, int mandatoryData = -1,ContainerSave? containerSave = null)
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

        entityCommandBuffer.AddComponent(e, new ServerEquipmentEventCounter() { index = uint.MaxValue });
        entityCommandBuffer.AppendToBuffer<PlayerContainers>(player, new PlayerContainers() { entity = e, index = index});
        
        
        entityCommandBuffer.SetBuffer<InventorySlot>(e).EnsureCapacity(capacity + 1);
        entityCommandBuffer.SetBuffer<ItemBarData>(e).EnsureCapacity(capacity + 1);
        entityCommandBuffer.AddComponent(e, new SendToOwner());

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
    }
    private void CreateNewContainer(Entity player,ref EntityCommandBuffer entityCommandBuffer,ref EntitiesReferences entities,int networkID, ContainerData data,ContainerSave? containerSave = null)
    {
        CreateNewContainer(player,ref entityCommandBuffer,ref entities,networkID,data.stats.waterResistance,data.stats.capacity,
        data.stats.containerIndex,data.stats.mandatoryProperties,data.stats.mandatoryData,containerSave);
    }
}
