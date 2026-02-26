using Game.Client.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class DebugManager : MonoBehaviour
{
    public const float updateInterval = 0.3f;

    [SerializeField] private GameObject linePrefab;
    [Space]
    [SerializeField] TextMeshProUGUI CPUStatsText;
    [SerializeField] TextMeshProUGUI GPUStatsText;
    [SerializeField] TextMeshProUGUI FPSCounterText;
    [SerializeField] TextMeshProUGUI PingCounterText;
    [Space]
    [SerializeField] TextMeshProUGUI gameVersionText;
    [Space]
    [SerializeField] TextMeshProUGUI playerPositionText;
    [SerializeField] TextMeshProUGUI enginePlayerPositionText;
    [SerializeField] TextMeshProUGUI chunkStatsText;

    Dictionary<int, Transform> debuggingChunks;
    World world;

    //FPS Counter 
    private int lastFrameIndex;
    private float[] frameDeltaTimeArray = new float[50];

    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
        debuggingChunks = new Dictionary<int, Transform>();
        world = MyTools.GetClientWorld();
        LoadDebugStats();
    }
    private void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (debuggingChunks.Count > 0)
            {
                TurnOffChunkDebugger();
            }
            else
            {
                ChunkDebugger();
            }
        }
    }
    private void OnDisable()
    {
        CollisionSystem.onPlayerMove -= UpdatePosition;
      //  GridVisualization.instance.onChangeChunk -= UpdateChunkDebugger;
        if (debuggingChunks.Count > 0) TurnOffChunkDebugger();
        StopCoroutine(UpdateStats());
    }

    private const float offset = 0.003f;
    private void TurnOffChunkDebugger()
    {
        foreach (var item in debuggingChunks)
        {
            if (item.Value != null) Destroy(item.Value.gameObject);
        }
        debuggingChunks.Clear();
    }
    private void ChunkDebugger()
    {
       // float distance = GridVisualization.instance.map.chunkSizeOnWorldScale;
        foreach (var item in GridVisualization.instance.loadedChunks)
        {
         //   debuggingChunks.Add(item.Key, CreateChunkBorder(item.Value.transform.position, distance));
        }
    }
    private Transform CreateChunkBorder(Transform lineTransform, Vector2 position, float chunkSizeInWorldScale)
    {
        LineRenderer line = lineTransform.GetComponent<LineRenderer>();
        line.positionCount = 5;
        line.SetPosition(0, new Vector3(position.x + offset - line.startWidth * 0.5f, position.y + offset, 0));
        line.SetPosition(1, new Vector3(position.x + chunkSizeInWorldScale - offset, position.y + offset, 0));
        line.SetPosition(2, new Vector3(position.x + chunkSizeInWorldScale - offset, position.y + chunkSizeInWorldScale - offset, 0));
        line.SetPosition(3, new Vector3(position.x + offset, position.y + chunkSizeInWorldScale - offset, 0));
        line.SetPosition(4, new Vector3(position.x + offset, position.y + offset, 0));
        line.GetComponent<SortingGroup>().sortingOrder = -1;
        return line.transform;
    }
    private Transform CreateChunkBorder(Vector2 position, float chunkSizeInWorldScale)
    {
        Transform line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).transform;
        return CreateChunkBorder(line, position, chunkSizeInWorldScale);
    }

    IEnumerator UpdateStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            UpdateDebugStats();
        }
    }
    private void LoadDebugStats()
    {
        SetUpDebugStats();
        StartCoroutine(UpdateStats());

        CollisionSystem.onPlayerMove += UpdatePosition;
        gameVersionText.text = Application.productName + " " + Application.version;
        var query = world.EntityManager.CreateEntityQuery(
         ComponentType.ReadOnly<Player>(),
         ComponentType.ReadOnly<LocalTransform>(),
         ComponentType.ReadOnly<GhostOwnerIsLocal>()
        );
        if (!query.IsEmpty)
        {
            var array = query.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.Temp);
            UpdatePosition(MyTools.ConvertFloat(array[0].Position));
            array.Dispose();
        }
        query.Dispose();

        //int chunkIndex = GridVisualization.instance.lastPlayerChunk;
        //  SetChunk(MapVisualization.instance.GetChunkCoordinates(chunkIndex), chunkIndex);
    }
    private float CalculateFPS()
    {
        float total = 0;
        foreach (float value in frameDeltaTimeArray)
        {
            total += value;
        }
        return frameDeltaTimeArray.Length / total;
    }


    private void SetUpDebugStats()
    {
        PingCounterText.gameObject.SetActive(GameInfo.instance.isMultiplayer);
        CPUStatsText.text = "???";
        GPUStatsText.text = "???";
        FPSCounterText.text = "???";
    }
    private void UpdateDebugStats()
    {
        float cpuUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024.0f * 1024.0f);
        CPUStatsText.text = string.Format("CPU Usage: {0:0.0} MB", cpuUsage);

        float gpuFrameTime = Time.deltaTime * 1000.0f;
        GPUStatsText.text = "GPU Frame Time: " + gpuFrameTime.ToString("F2") + " ms";
        FPSCounterText.text = "FPS: " + ((int)CalculateFPS()).ToString();


        var entityManager = world.EntityManager;

        var query = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<NetworkStreamConnection>(),
            ComponentType.ReadOnly<NetworkSnapshotAck>()
        );
        if (query.IsEmpty)
        {
            return;
        }

        var pingArray = query.ToComponentDataArray<NetworkSnapshotAck>(Unity.Collections.Allocator.Temp);
        PingCounterText.text = "Ping: " + pingArray.FirstOrDefault().EstimatedRTT.ToString("F2") + " ms";

        query.Dispose();
        pingArray.Dispose();
    }
    private void UpdatePosition(float2 postion)
    {
        enginePlayerPositionText.text = ($"Engine position: [ x:{postion.x.ToString("F1")} y:{postion.y.ToString("F1")} ]");
        int2 mapPos =  MapVisualization.instance.clientMap.EnginePositionToMapPos(postion);
        playerPositionText.text = ($"Player position: [ x:{mapPos.x} y:{mapPos.y} ]");
        int2 chunkCoords = ClientMap.MapPosToChunkCoordinates(mapPos);
        int index = MapVisualization.instance.clientMap.ChunkCoordiantesToChunkIndex(chunkCoords);
        chunkStatsText.text = ($"Chunk: [ x:{chunkCoords.x} y:{chunkCoords.y} ] Index: {index}");
    }
    private void UpdateChunkDebugger(object sender, PlayerPositionArgs e)
    {
        float distance = GridVisualization.instance.map.chunkSizeOnWorldScale;
        List<int> freeLines = new List<int>();

        foreach (var value in debuggingChunks)
        {
            if (!GridVisualization.instance.loadedChunks.ContainsKey(value.Key))
            {
                freeLines.Add(value.Key);
            }
        }
        foreach (var value in GridVisualization.instance.loadedChunks)
        {
            if (!debuggingChunks.ContainsKey(value.Key))
            {
                Transform line;
                if (freeLines.Count > 0)
                {
                    int index = freeLines[0];
         //           line = CreateChunkBorder(debuggingChunks[index], value.Value.transform.position, distance);
                    debuggingChunks.Remove(index);
                    freeLines.RemoveAt(0);
                }
                else
                {
           //         line = CreateChunkBorder(value.Value.transform.position, distance);
                }
         //       debuggingChunks.Add(value.Key, line);
            }
        }
        foreach (var item in freeLines)
        {
            Destroy(debuggingChunks[item].gameObject);
            debuggingChunks.Remove(item);
        }
    }

    private void SetChunk(int2 chunkCoordinates, int chunkIndex)
    {
        chunkStatsText.text = ($"Chunk: [ {chunkCoordinates.x} , {chunkCoordinates.y} ]  Index: {chunkIndex}");
    }
}
