using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;

public struct SelectedSlot : IComponentData
{
    public SlotPosition Position;
}