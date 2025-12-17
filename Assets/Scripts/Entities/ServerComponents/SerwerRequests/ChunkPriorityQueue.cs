





using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;


public interface IPriority
{
    public int priority {get;}
}

public struct PriorityQueue<T> where T : unmanaged, IPriority
{
    private NativeList<(T value,Entity entity)> heap;
    public PriorityQueue(int capacity, Allocator allocator)
    {
        heap = new NativeList<(T,Entity)>(capacity, allocator);
    }

    public int Count => heap.Length;

    public bool TryPop(out (T value,Entity entity) result)
    {
        if (heap.Length == 0)
        {
            result = default;
            return false;
        }
        result = heap[0];
        heap[0] = heap[heap.Length - 1];
        heap.RemoveAt(heap.Length - 1);
        HeapifyDown(0);
        return true;
    }
    public void Push(T req, Entity entity)
    {
        heap.Add((req,entity));
        HeapifyUp(heap.Length - 1);
    }
    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (heap[index].value.priority >= heap[parent].value.priority)
                break;

            (heap[index], heap[parent]) = (heap[parent], heap[index]);
            index = parent;
        }
    }
    private void HeapifyDown(int index)
    {
        int last = heap.Length - 1;
        while (true)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            int smallest = index;

            if (left <= last && heap[left].value.priority < heap[smallest].value.priority)
                smallest = left;
            if (right <= last && heap[right].value.priority < heap[smallest].value.priority)
                smallest = right;

            if (smallest == index)
                break;

            (heap[index], heap[smallest]) = (heap[smallest], heap[index]);
            index = smallest;
        }
    }
    public void Dispose()
    {
        heap.Dispose();
    }
}
