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

    [FieldOffset( 16)] public FixedTileRow list0;              
    [FieldOffset( 96)] public FixedTileRow list1;              
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

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 80)]
[GenerateTestsForBurstCompatibility]
public struct FixedBytes : IRpcCommand
{
    [FieldOffset(0)]  public long bytes0;
    [FieldOffset(8)]  public long bytes1;
    [FieldOffset(16)] public long bytes2;
    [FieldOffset(24)] public long bytes3;
    [FieldOffset(32)] public long bytes4;
    [FieldOffset(40)] public long bytes5;
    [FieldOffset(48)] public long bytes6;
    [FieldOffset(56)] public long bytes7;
    [FieldOffset(64)] public long bytes8;
    [FieldOffset(72)] public long bytes9;

    public byte this[int i]
    {
        get
        {
            int index = i % 8;
            switch (i / 8)
            {
                case 0: return GetByte(bytes0, index);
                case 1: return GetByte(bytes1, index);
                case 2: return GetByte(bytes2, index);
                case 3: return GetByte(bytes3, index);
                case 4: return GetByte(bytes4, index);
                case 5: return GetByte(bytes5, index);
                case 6: return GetByte(bytes6, index);
                case 7: return GetByte(bytes7, index);
                case 8: return GetByte(bytes8, index);
                case 9: return GetByte(bytes9, index);
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            int index = i % 8;
            switch (i / 8)
            {
                case 0: SetByte(ref bytes0, index, value); break;
                case 1: SetByte(ref bytes1, index, value); break;
                case 2: SetByte(ref bytes2, index, value); break;
                case 3: SetByte(ref bytes3, index, value); break;
                case 4: SetByte(ref bytes4, index, value); break;
                case 5: SetByte(ref bytes5, index, value); break;
                case 6: SetByte(ref bytes6, index, value); break;
                case 7: SetByte(ref bytes7, index, value); break;
                case 8: SetByte(ref bytes8, index, value); break;
                case 9: SetByte(ref bytes9, index, value); break;

                default: throw new IndexOutOfRangeException();
            }
        }
    }

        private byte GetByte(long bytes,int index)
        {
           return BitConverter.GetBytes(bytes)[index];
        }
    
        private void SetByte(ref long bytes, int index, byte value)
        {
            byte[] array = BitConverter.GetBytes(bytes);
            array[index] = value;
            bytes = BitConverter.ToInt64(array, 0);
        }
    
}

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 800)]
[GenerateTestsForBurstCompatibility]
public struct Fixed100Bytes : IRpcCommand
{
    [FieldOffset(0)]  public FixedBytes bytes0;
    [FieldOffset(80)] public FixedBytes bytes1;
    [FieldOffset(160)] public FixedBytes bytes2;
    [FieldOffset(240)] public FixedBytes bytes3;
    [FieldOffset(320)] public FixedBytes bytes4;
    [FieldOffset(400)] public FixedBytes bytes5;
    [FieldOffset(480)] public FixedBytes bytes6;
    [FieldOffset(560)] public FixedBytes bytes7;
    [FieldOffset(640)] public FixedBytes bytes8;
    [FieldOffset(720)] public FixedBytes bytes9;

    public byte this[int index]
    {
        get
        {
            int i = index % 10;
            switch (index/10)
            {
                case 0: return bytes0[i];
                case 1: return bytes1[i];
                case 2: return bytes2[i];
                case 3: return bytes3[i];
                case 4: return bytes4[i];
                case 5: return bytes5[i];
                case 6: return bytes6[i];
                case 7: return bytes7[i];
                case 8: return bytes8[i];
                case 9: return bytes9[i];
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            int i = index % 10;
            switch (index/10)
            {
                case 0: bytes0[i] = value; break;
                case 1: bytes1[i] = value; break;
                case 2: bytes2[i] = value; break;
                case 3: bytes3[i] = value; break;
                case 4: bytes4[i] = value; break;
                case 5: bytes5[i] = value; break;
                case 6: bytes6[i] = value; break;
                case 7: bytes7[i] = value; break;
                case 8: bytes8[i] = value; break;
                case 9: bytes9[i] = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}




