using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


[UpdateInGroup(typeof(GhostInputSystemGroup),OrderFirst = true)]
partial struct PlayerInputSystem : ISystem
{   
    public static event Action<int> onNewSlotInHand;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<PlayerInput>();
    }
    public void OnUpdate(ref SystemState state)
    {
        float2 input = (float2)InputManager.i.move.ReadValue<Vector2>();

        bool left = InputManager.i.mainAction.inProgress;
        bool right = InputManager.i.sideAction.inProgress;

        if (math.lengthsq(input) > 1) input = math.normalize(input);

        float3 target = (float3)MyTools.GetMouseWorldPosition();
        float2 sightDirection = new float2(target.x,target.y);

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (!WindowsManager.instance.CloseOpenWindows())
        //        WindowsManager.instance.LoadScene(11);
        //}


        if (InputManager.i.playerList.triggered && !ChatManager.instance.isChatting)
        {
            WindowsManager.instance.LoadScene(9);
        }


        foreach ((RefRW<PlayerInput> playerInput, RefRW<PlayerInputSync> playerInputSync , RefRW<Hands> hands, Entity entity) in 
            SystemAPI.Query<RefRW<PlayerInput>, RefRW<PlayerInputSync>, RefRW<Hands>>().WithAll<GhostOwnerIsLocal,Simulate>().WithNone<NewPlayerTag>().WithEntityAccess())
        {
            playerInput.ValueRW.movementDirection = input;
            playerInputSync.ValueRW.movementDir = input;

            playerInput.ValueRW.sightDirection = sightDirection;
            playerInputSync.ValueRW.sightDirection = sightDirection;

            if (left)
            {
                playerInput.ValueRW.leftButton.Set();
                playerInputSync.ValueRW.leftButton.Set();
            }
            else
            {
                playerInput.ValueRW.leftButton = default;
                playerInputSync.ValueRW.leftButton = default;
            }

            if (right)
            {
                playerInput.ValueRW.rightButton.Set();
                playerInputSync.ValueRW.rightButton.Set();

                quaternion quaternion = SystemAPI.GetComponent<LocalTransform>(hands.ValueRO.main).Rotation;
                //playerInput.ValueRW.handRotation = quaternion;
                //playerInputSync.ValueRW.handRotation = quaternion;
            }
            else
            {
                playerInput.ValueRW.rightButton = default;
                playerInputSync.ValueRW.rightButton = default;
            }

            int newSlot = InputManager.i.GetNextSlotInHand(playerInput.ValueRO.slotInHand);
            if(playerInput.ValueRO.slotInHand != newSlot)
            {
                onNewSlotInHand?.Invoke(newSlot);
                playerInput.ValueRW.slotInHand = newSlot;

                CharacterManager.instance.ChangeItemInHand(newSlot, entity, ref state);
            }
        }
    }
}
