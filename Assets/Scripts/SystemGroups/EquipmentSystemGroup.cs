using Unity.Entities;
using Unity.NetCode;

public partial class EquipmentSystemGroup : ComponentSystemGroup
{
    
}



[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
[UpdateBefore(typeof(DestroyEntitySystem))]
public partial class DestroySystemGroup : ComponentSystemGroup
{
    
}
