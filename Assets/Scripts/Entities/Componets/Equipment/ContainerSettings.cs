using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;

public struct ContainerSettings : IComponentData
{
    public SlotPosition Position;
    public int targetContainer;
}


public struct NextTempIndex : IComponentData
{
    public int nextTempIndex;
}