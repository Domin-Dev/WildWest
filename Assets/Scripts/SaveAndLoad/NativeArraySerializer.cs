
using Unity.Collections;
using System;
using Unity.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

public static class NativeArraySerializer
{
    public static byte[] ToBytes<T>(NativeArray<T> array) where T : unmanaged
    {
        Debug.Log(MemoryMarshal.AsBytes(array.AsSpan()).Length);
        return MemoryMarshal.AsBytes(array.AsSpan()).ToArray();
    }
    public static NativeArray<T> FromBytes<T>(byte[] bytes, Allocator allocator) where T : struct
    {
        int size = UnsafeUtility.SizeOf<T>();
        if (bytes.Length % size != 0)
            throw new ArgumentException("Invalid byte array length.");

        int count = bytes.Length / size;
        var array = new NativeArray<T>(count, allocator, NativeArrayOptions.UninitializedMemory);
        MemoryMarshal.Cast<byte, T>(bytes).CopyTo(array);
        return array;  
    }

    public static byte[] StructToBytes<T>(T value) where T : struct
    {
        int size = UnsafeUtility.SizeOf<T>();
        byte[] buffer = new byte[size];
        MemoryMarshal.Write(buffer, ref value);
        return buffer;
    }

    public static T BytesToStruct<T>(ReadOnlySpan<byte> span) where T : struct
    {
        if (span.Length < UnsafeUtility.SizeOf<T>())
            throw new ArgumentException("Buffer too small.");

        return MemoryMarshal.Read<T>(span);
    }


}
