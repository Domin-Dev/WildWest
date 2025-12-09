using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;



public struct ServerData : IComponentData
{
    public FixedString128Bytes hash;
    public bool isPassword;
    public bool isHost;
    public int playersLimit;
    public int hostNetworkID;
    public bool blackList;
}