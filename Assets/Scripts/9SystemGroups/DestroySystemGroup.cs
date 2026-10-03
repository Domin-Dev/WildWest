using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup),OrderLast = true)]
[UpdateBefore(typeof(DestroyEntitySystem))]
public partial class DestroySystemGroup : ComponentSystemGroup
{
    
}