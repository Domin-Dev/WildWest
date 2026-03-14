
using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PredictionSmoothingSystem : ISystem
{

    public static bool enabled = true;

    public void OnCreate(ref SystemState state)
    {
        if(!enabled)
        {
            return;
        }
        var ghostPredictionSmoothing = SystemAPI.GetSingleton<GhostPredictionSmoothing>();
        ghostPredictionSmoothing.RegisterSmoothingAction<LocalTransform>(state.EntityManager, CustomSmoothing.Action);
    }




    [BurstCompile]
    public unsafe class CustomSmoothing
    {
        public const float delta = 0.5f;
        public const float maxDistance = 0.2f;
    

        public static readonly PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>
            Action =
                new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(SmoothingAction);

        [BurstCompile(DisableDirectCall = true)]
        private static void SmoothingAction(IntPtr currentData, IntPtr previousData, IntPtr userData)
        {
            ref var trans = ref UnsafeUtility.AsRef<LocalTransform>(currentData.ToPointer());
            ref var backup = ref UnsafeUtility.AsRef<LocalTransform>(previousData.ToPointer());

            var dist = math.distance(trans.Position, backup.Position);

           // if (dist < maxDistance && dist > delta && dist > 0)
        //   if(dist > 0)
           //     trans.Position = backup.Position + (trans.Position - backup.Position) * delta / dist;
        }
    }

}