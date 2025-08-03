using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 804)]
[GenerateTestsForBurstCompatibility]
public struct PingList : IRpcCommand
{

    [FieldOffset(0)]   public short length;
    [FieldOffset(2)]   public PingSet player0;
    [FieldOffset(82)]  public PingSet player1;
    [FieldOffset(162)] public PingSet player2;
    [FieldOffset(242)] public PingSet player3;
    [FieldOffset(322)] public PingSet player4;
    [FieldOffset(402)] public PingSet player5;
    [FieldOffset(482)] public PingSet player6;
    [FieldOffset(562)] public PingSet player7;
    [FieldOffset(642)] public PingSet player8;
    [FieldOffset(722)] public PingSet player9;

    public PingData this[int x]
    {
        get
        {
            int y = x % 10;
            switch (x / 10)
            {
                case 0: return player0[y];
                case 1: return player1[y];
                case 2: return player2[y];
                case 3: return player3[y];
                case 4: return player4[y];
                case 5: return player5[y];
                case 6: return player6[y];
                case 7: return player7[y];
                case 8: return player8[y];
                case 9: return player9[y];
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            int y = x % 10;

            switch (x / 10)
            {
                case 0: player0[y] = value; break;
                case 1: player1[y] = value; break;
                case 2: player2[y] = value; break;
                case 3: player3[y] = value; break;
                case 4: player4[y] = value; break;
                case 5: player5[y] = value; break;
                case 6: player6[y] = value; break;
                case 7: player7[y] = value; break;
                case 8: player8[y] = value; break;
                case 9: player9[y] = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 80)]
[GenerateTestsForBurstCompatibility]
public struct PingSet
{
    [FieldOffset(0)] public PingData player0;
    [FieldOffset(8)] public PingData player1;
    [FieldOffset(16)] public PingData player2;
    [FieldOffset(24)] public PingData player3;
    [FieldOffset(32)] public PingData player4;
    [FieldOffset(40)] public PingData player5;
    [FieldOffset(48)] public PingData player6;
    [FieldOffset(56)] public PingData player7;
    [FieldOffset(64)] public PingData player8;
    [FieldOffset(72)] public PingData player9;

    public PingData this[int x]
    {
        get
        {
            switch (x)
            {
                case 0: return player0;
                case 1: return player1;
                case 2: return player2;
                case 3: return player3;
                case 4: return player4;
                case 5: return player5;
                case 6: return player6;
                case 7: return player7;
                case 8: return player8;
                case 9: return player9;
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            switch (x)
            {
                case 0: player0 = value; break;
                case 1: player1 = value; break;
                case 2: player2 = value; break;
                case 3: player3 = value; break;
                case 4: player4 = value; break;
                case 5: player5 = value; break;
                case 6: player6 = value; break;
                case 7: player7 = value; break;
                case 8: player8 = value; break;
                case 9: player9 = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }

}


[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 8)]
[GenerateTestsForBurstCompatibility]
public struct PingData
{
    [FieldOffset(0)] public int playerID;
    [FieldOffset(4)] public int ping;
}