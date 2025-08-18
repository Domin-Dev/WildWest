using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public class HeaderData
{
    public FixedString128Bytes playerName;
    public Difficulty difficulty;
    public string worldName;
    public long creationTime;
    public long saveTime;
    public CharacterLook characterLook;
}
