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
[StructLayout(LayoutKind.Explicit, Size = 1084)]
[GenerateTestsForBurstCompatibility]
public struct PlayerList : IRpcCommand
{
    [FieldOffset(0)] public short max;
    [FieldOffset(2)]   public PlayerData player0;
    [FieldOffset(182)] public PlayerData player1;
    [FieldOffset(362)] public PlayerData player2;
    [FieldOffset(542)] public PlayerData player3;
    [FieldOffset(722)] public PlayerData player4;
    [FieldOffset(902)] public PlayerData player5;
    public PlayerData this[int x]
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
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}


[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 180)]
[GenerateTestsForBurstCompatibility]
public struct PlayerData
{
    [FieldOffset(0)] public FixedString128Bytes playerName;
    [FieldOffset(128)] public int playerID;
    [FieldOffset(132)] public CharacterLook characterLook;
}