
using Unity.Entities;
using Unity.Mathematics;

public struct GhostChunk : IComponentData
{
    public int spawnChunk;
    public int current;
    public int lastChunk;
    
    public Entity currentChunkEntity;
    public float3 lastPosition;

    public readonly static float3 incorrectPosition = new float3(float.MinValue,float.MinValue,float.MinValue);

    public GhostChunk StartValues()
    {
        current = int.MinValue;
        lastChunk = int.MinValue;
        spawnChunk = int.MinValue;
        lastPosition = incorrectPosition;
        return this;
    }

    public GhostChunk(GhostChunk ghostChunk)
    {
        this.current = ghostChunk.current;
        this.spawnChunk = ghostChunk.spawnChunk;
        this.lastChunk = ghostChunk.lastChunk;
        this.currentChunkEntity = ghostChunk.currentChunkEntity;
        this.lastPosition = ghostChunk.lastPosition;
    }
    
    public bool LastPositionIsCorrect()
    {
        return lastPosition.x != incorrectPosition.x;
    }

    public void SetChunkEntity(Entity entity)
    {
        this.currentChunkEntity = entity;
    }
    public int GetLastChunk()
    {
        if(LastChunkIsNull() && !SpawnChunkIsNull())
        {
            int spawn = spawnChunk;
            spawnChunk = int.MinValue;
            return spawn;
        }
        return lastChunk;
    }
    public int GetChunk()
    {
        if(CurrentChunkIsNull())
            return spawnChunk;
        else
            return current;
    }

    public bool HasChunk()
    {
        return GetChunk() != int.MinValue;
    }

    public bool SpawnChunkIsNull()
    {
        return current == int.MinValue;
    }
    public bool CurrentChunkIsNull()
    {
        return current == int.MinValue;
    }
    public bool LastChunkIsNull()
    {
        return lastChunk == int.MinValue;
    }
    public void SetNewChunk(int newChunk = int.MinValue)
    {
        lastChunk = current;
        current = newChunk;
    }
}

