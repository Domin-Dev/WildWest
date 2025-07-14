using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


public struct RPCSendQueue : IComponentData
{
    public NetworkTick tick;
    public Entity target;
}