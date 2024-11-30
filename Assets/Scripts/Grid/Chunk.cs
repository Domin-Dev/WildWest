using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkItem
{
    public ItemStats item;
    public Vector2 position;
    public Transform worldItem;

    public ChunkItem(ItemStats item,Vector2 position, Transform worldItem)
    {
        this.item = item;
        this.position = position;
        this.worldItem = worldItem;
    }
}
public class Chunk
{
    public GridTile[,] grid;
    public Vector2 ChunkGridPosition { set; get; }
    public Vector2 position { set; get; }

    public List<ChunkItem> items { set; get;}
    public Chunk(int gridSize,Vector2 chunkCoordinates, Vector2 position)
    {
        this.grid = new GridTile[gridSize,gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                grid[i, j] = new GridTile((int)chunkCoordinates.x + i,(int)chunkCoordinates.y + j);
            }
        }
        this.ChunkGridPosition = chunkCoordinates;
        this.position = position;
        items = new List<ChunkItem>();
    }
    public int AddItem(ChunkItem chunkItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if(items[i] == null)
            {
                items[i] = chunkItem;
                return i;
            }
        }
        items.Add(chunkItem);
        return items.Count - 1;
    }
    public void RemoveItem(int index)
    {
        if (index == items.Count - 1)
        {
            items.RemoveAt(index);
        }
        else
        {
            items[index] = null;
        }
    }

    public ChunkItem FindItem(Vector2 position, int id, int itemChunkIndex)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i == itemChunkIndex) continue;
            ChunkItem chunkItem = items[i];
            if (chunkItem != null && chunkItem.position == position && chunkItem.item.itemID == id)
            {
                if (chunkItem.item.itemCount < ItemsAsset.instance.GetStackMax(id)) 
                    return items[i];
            }
        }
        return null;
    }

    public void MoveAllItems(Vector2 posXY, Vector2 newPos)
    {
        for (int i = 0; i < items.Count; i++)
        {
            ChunkItem chunkItem = items[i];
            if (chunkItem != null && chunkItem.position == posXY)
            {
               // chunkItem.worldItem;
            }
        }
    }

    public void MoveAllItems(Vector2 posXY, Vector2 newPos,Chunk newChunk)
    {
    }
}

