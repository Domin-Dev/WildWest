using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct Rain : IComponentData
{
    public float intensity; // 0 - 1
    public float time;
    public NetworkTick tick;
}



