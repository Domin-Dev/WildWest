using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;


[UpdateInGroup(typeof(GhostInputSystemGroup),OrderFirst = true)]
partial struct PlayerInputSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<PlayerInput>();
    }
    public void OnUpdate(ref SystemState state)
    {


        float2 input = float2.zero;

        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.W)) input.y += 1;

        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;

        bool left = Input.GetMouseButton(0);
        bool right = Input.GetMouseButton(1);


        if (math.lengthsq(input) > 1) input = math.normalize(input);

        float3 target = (float3)MyTools.GetMouseWorldPosition();
        float2 sightDirection = new float2(target.x,target.y);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadScene(8);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            LoadScene(9);
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
                playerInput.ValueRW.handRotation = quaternion;
                playerInputSync.ValueRW.handRotation = quaternion;
            }
            else
            {
                playerInput.ValueRW.rightButton = default;
                playerInputSync.ValueRW.rightButton = default;
            }



            playerInput.ValueRW.dataTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            //var inputBuffer = state.EntityManager.GetBuffer<InputBufferData<PlayerInput>>(entity);


            //Debug.Log("nowe " + tick.TickValue + " " + playerInput.ValueRO);

            //inputBuffer.AddCommandData(new InputBufferData<PlayerInput>
            //{
            //    Tick = tick,
            //    InternalInput = new PlayerInput
            //    {
            //        movementDirection = playerInput.ValueRO.movementDirection,
            //        leftButton = playerInput.ValueRW.leftButton,
            //        rightButton = playerInput.ValueRW.rightButton,
            //        handRotation = playerInput.ValueRO.handRotation,
            //        sightDirection = playerInput.ValueRO.sightDirection
            //    }
            //});
        }
    }


    private void LoadScene(int index)
    {
        bool load = true;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == index && scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(index);
                load = false;
            }
        }
        if (load)
            SceneManager.LoadScene(index, LoadSceneMode.Additive);
    }
}
