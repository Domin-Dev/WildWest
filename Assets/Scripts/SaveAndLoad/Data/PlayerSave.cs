using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public class PlayerSave
{
    public FixedString128Bytes playerName;
    public CharacterLook characterLook;
    public float2 playerPosition;

    public int health;
    public int hunger;
    public int thirst;
}