using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;


[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 824)]
[GenerateTestsForBurstCompatibility]
public struct FixedBuildingObjects : IRpcCommand
{
    public const int size = 800;

    [FieldOffset(0)] public float2 worldPosition;
    [FieldOffset(8)] public int2 chunkCoordinates;

    [FieldOffset(16)] public FixedBytes list0;
    [FieldOffset(96)] public FixedBytes list1;
    [FieldOffset(176)] public FixedBytes list2;
    [FieldOffset(256)] public FixedBytes list3;
    [FieldOffset(336)] public FixedBytes list4;
    [FieldOffset(416)] public FixedBytes list5;
    [FieldOffset(496)] public FixedBytes list6;
    [FieldOffset(576)] public FixedBytes list7;
    [FieldOffset(656)] public FixedBytes list8;
    [FieldOffset(736)] public FixedBytes list9;

    [FieldOffset(816)] public uint number;

    public byte this[int i]
    {
        get
        {
            int index = i % 80;
            switch (i / 80)
            {
                case 0: return list0[index];
                case 1: return list1[index];
                case 2: return list2[index];
                case 3: return list3[index];
                case 4: return list4[index];
                case 5: return list5[index];
                case 6: return list6[index];
                case 7: return list7[index];
                case 8: return list8[index];
                case 9: return list9[index];

                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            int index = i % 80;
            switch (i / 80)
            {
                case 0: list0[index] = value; break;
                case 1: list1[index] = value; break;
                case 2: list2[index] = value; break;
                case 3: list3[index] = value; break;
                case 4: list4[index] = value; break;
                case 5: list5[index] = value; break;
                case 6: list6[index] = value; break;
                case 7: list7[index] = value; break;
                case 8: list8[index] = value; break;
                case 9: list9[index] = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }

}




