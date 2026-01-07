
using Unity.Collections;
using System;

using Unity.Collections;
using System;
using System.Runtime.InteropServices;

public static class NativeArraySerializer
{
    public static byte[] ToBytes<T>(NativeArray<T> array) where T : struct
    {
        T[] managed = array.ToArray();
        int size = Marshal.SizeOf<T>() * managed.Length;
        byte[] bytes = new byte[size];

        for (int i = 0; i < managed.Length; i++)
        {
            byte[] elementBytes = StructToBytes(managed[i]);
            Buffer.BlockCopy(elementBytes, 0, bytes, i * elementBytes.Length, elementBytes.Length);
        }
        return bytes;
    }

    public static NativeArray<T> FromBytes<T>(byte[] bytes, Allocator allocator) where T : struct
    {
        int sizeOfT = Marshal.SizeOf<T>();
        int count = bytes.Length / sizeOfT;

        T[] managed = new T[count];

        for (int i = 0; i < count; i++)
        {
            byte[] elementBytes = new byte[sizeOfT];
            Buffer.BlockCopy(bytes, i * sizeOfT, elementBytes, 0, sizeOfT);
            managed[i] = BytesToStruct<T>(elementBytes);
        }

        return new NativeArray<T>(managed, allocator);
    }

    private static byte[] StructToBytes<T>(T str) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    private static T BytesToStruct<T>(byte[] arr) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, 0, ptr, size);
        T str = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);
        return str;
    }
}
