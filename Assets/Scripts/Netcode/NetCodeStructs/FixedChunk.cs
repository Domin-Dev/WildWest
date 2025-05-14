using System.Drawing;
using System.Runtime.InteropServices;
using System;
using Unity.Collections;
using Unity.NetCode;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Unity.Entities;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 816)]
[GenerateTestsForBurstCompatibility]
public struct FixedChunk : IRpcCommand
{
    [FieldOffset(0)] public float2 worldPosition;             
    [FieldOffset(8)] public int2 chunkCoordinates;           

    [FieldOffset(16)] public FixedTileRow list0;              
    [FieldOffset(96)] public FixedTileRow list1;              
    [FieldOffset(176)] public FixedTileRow list2;             
    [FieldOffset(256)] public FixedTileRow list3;
    [FieldOffset(336)] public FixedTileRow list4;
    [FieldOffset(416)] public FixedTileRow list5;
    [FieldOffset(496)] public FixedTileRow list6;
    [FieldOffset(576)] public FixedTileRow list7;
    [FieldOffset(656)] public FixedTileRow list8;
    [FieldOffset(736)] public FixedTileRow list9;

    public FixedTile this[int x,int y]
    {
        get
        {
            switch (x)
            {
                case 0: return list0[y];
                case 1: return list1[y];
                case 2: return list2[y];
                case 3: return list3[y];
                case 4: return list4[y];
                case 5: return list5[y];
                case 6: return list6[y];
                case 7: return list7[y];
                case 8: return list8[y];
                case 9: return list9[y];
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            switch (x)
            {
                case 0: list0[y] = value; break;
                case 1: list1[y] = value; break;
                case 2: list2[y] = value; break;
                case 3: list3[y] = value; break;
                case 4: list4[y] = value; break;
                case 5: list5[y] = value; break;
                case 6: list6[y] = value; break;
                case 7: list7[y] = value; break;
                case 8: list8[y] = value; break;
                case 9: list9[y] = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}


[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 80)]
[GenerateTestsForBurstCompatibility]
public struct FixedTileRow : IRpcCommand
{
    [FieldOffset(0)]  public FixedTile tile0;
    [FieldOffset(8)]  public FixedTile tile1;
    [FieldOffset(16)] public FixedTile tile2;
    [FieldOffset(24)] public FixedTile tile3;
    [FieldOffset(32)] public FixedTile tile4;
    [FieldOffset(40)] public FixedTile tile5;
    [FieldOffset(48)] public FixedTile tile6;
    [FieldOffset(56)] public FixedTile tile7;
    [FieldOffset(64)] public FixedTile tile8;
    [FieldOffset(72)] public FixedTile tile9;

    public FixedTile this[int index]
    {
        get => index switch
        {
            0 => tile0,
            1 => tile1,
            2 => tile2,
            3 => tile3,
            4 => tile4,
            5 => tile5,
            6 => tile6,
            7 => tile7,
            8 => tile8,
            9 => tile9,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0: tile0 = value; break;
                case 1: tile1 = value; break;
                case 2: tile2 = value; break;
                case 3: tile3 = value; break;
                case 4: tile4 = value; break;
                case 5: tile5 = value; break;
                case 6: tile6 = value; break;
                case 7: tile7 = value; break;
                case 8: tile8 = value; break;
                case 9: tile9 = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}


[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 8)]
[GenerateTestsForBurstCompatibility]
public struct FixedTile 
{
    [FieldOffset(0)] public int tileID;
    [FieldOffset(4)] public byte variant;

    public FixedTile(int tileID, byte variant)
    {
        this.tileID = tileID;
        this.variant = variant; 
    }

    public FixedTile(GridTile tile)
    {
        this.tileID = tile.tileID;
        this.variant = (byte)tile.variant;
    }
}

