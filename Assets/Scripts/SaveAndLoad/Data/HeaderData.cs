using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public struct HeaderData
{
    public FixedString128Bytes worldName;
    public FixedString128Bytes playerName;
    public Difficulty difficulty;
    public int seed;
    public long creationTime;
    public long saveTime;
    public double playTime;
}
