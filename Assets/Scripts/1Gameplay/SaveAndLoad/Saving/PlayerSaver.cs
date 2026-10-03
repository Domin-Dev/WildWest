
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class PlayerSaver : IndexedDataSaver<PlayerSave, ContainerSave, string>
{
    public PlayerSaver(string directoryPath, string extension = "dat") : base(directoryPath, extension){}

    protected override bool Reading(MemoryStream ms, BinaryReader reader, string fileIndex, out PlayerSave[] data1, out ContainerSave[] data2)
    {
        ms.Seek(0, SeekOrigin.Begin);
        data1 = new PlayerSave[]{NativeArraySerializer.BytesToStruct<PlayerSave>(reader.ReadBytes(Marshal.SizeOf<PlayerSave>()))};
        List<ContainerSave> containers = new List<ContainerSave>();

        while(ms.Position < ms.Length)
        {
            ContainerSave container;
            container.containerIndex = reader.ReadInt32();
            container.capacity = reader.ReadInt32();
            container.slots = NativeArraySerializer.FromBytes<SlotSave>(reader.ReadBytes(UnsafeUtility.SizeOf<SlotSave>() * container.capacity),Allocator.Persistent);
            container.barData = NativeArraySerializer.FromBytes<BarDataSave>(reader.ReadBytes(UnsafeUtility.SizeOf<BarDataSave>() * container.capacity),Allocator.Persistent);
            containers.Add(container);
        }
        
        data2 = containers.ToArray();
        return true;
    }
    protected override void Writing(MemoryStream ms, BinaryReader reader, BinaryWriter writer, string fileIndex, PlayerSave[] data1, params ContainerSave[] data2)
    {
        ms.Seek(0,SeekOrigin.Begin);
        if(ms.Length < UnsafeUtility.SizeOf<PlayerSave>())
        {
            if(data1 == null || data1.Length == 0)
                throw new InvalidDataException("File is empty.");
            else
                writer.Write(NativeArraySerializer.StructToBytes(data1[0]));
        }
        else if(data1 != null && data1.Length != 0)
            writer.Write(NativeArraySerializer.StructToBytes(data1[0]));


        if(data2 != null)
        {
            int offset = Marshal.SizeOf<PlayerSave>();
            ms.Seek(offset,SeekOrigin.Begin);
            int counter = 0;
            while(counter < data2.Length)
            {
                if(ms.Length > ms.Position)
                {
                    int contIndex = reader.ReadInt32();
                    int capacity = reader.ReadInt32();
                    for(int i = 0; i < data2.Length; i++)
                    {
                        ContainerSave container = data2[i];
                        if(container.containerIndex < 0 ) 
                            continue;

                        if(contIndex == container.containerIndex && capacity == container.capacity)
                        {
                            writer.Write(NativeArraySerializer.ToBytes(container.slots));
                            writer.Write(NativeArraySerializer.ToBytes(container.barData));
                            data2[i].containerIndex = -1;
                            counter++;
                            break;
                        }
                    }
                }
                else
                {
                    for(int i = 0; i < data2.Length; i++)
                    {
                        ContainerSave container = data2[i];
                        if(container.containerIndex < 0 ) 
                            continue;

                        writer.Write(container.containerIndex);
                        writer.Write(container.capacity);
                        writer.Write(NativeArraySerializer.ToBytes(container.slots));
                        writer.Write(NativeArraySerializer.ToBytes(container.barData));
                        counter++;
                    }
                }
            }  
        }    
        ms.Flush();
    }
}