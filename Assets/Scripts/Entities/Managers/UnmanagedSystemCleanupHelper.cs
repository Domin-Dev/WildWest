using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


public class UnmanagedSystemCleanupHelper : MonoBehaviour
{
    private List<SystemHandle> toDestroy = new();

    public static UnmanagedSystemCleanupHelper Instance
    {
        get 
        { 
            if (i == null) 
                i = new GameObject("Helper",typeof(UnmanagedSystemCleanupHelper)).GetComponent<UnmanagedSystemCleanupHelper>();
            return i;
        }
    }

    private static UnmanagedSystemCleanupHelper i;

    public void ScheduleDestroy(SystemHandle systemHandle)
    {
        toDestroy.Add(systemHandle);
    }

    void LateUpdate()
    {
        var world = World.DefaultGameObjectInjectionWorld.Unmanaged;

        foreach (var handle in toDestroy)
        {
            Debug.Log($"[Cleanup] Destroying unmanaged system: {handle}");
            ClientServerBootstrap.ClientWorld.DestroySystem(handle);
        }

        toDestroy.Clear();
    }
}