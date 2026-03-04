using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class EquipmentConfig : MonoBehaviour
{
    private static EquipmentConfig i;
    private Dictionary<int,ContainerData> containers;



    public LocalizedString defaultContainerName {private set;get;}
    public IReadOnlyDictionary<int, ContainerData> Containers => containers;
    public int itemInHand_ContainerIndex {private set;get;}
    public int hotBar_ContainerIndex {private set;get;}



    public static EquipmentConfig Instance
    {
        get
        {
            if (i == null)
            {
                i = new GameObject("EquipmentConfig", typeof(EquipmentConfig)).GetComponent<EquipmentConfig>();
            }
            return i;
        }
    }
    
    public void Awake()
    {
        var config = Resources.Load<EquipmentConfigData>("Config/EquipmentConfig");
        defaultContainerName = config.defaultContainerName;
        containers = new Dictionary<int, ContainerData>();

        itemInHand_ContainerIndex = config.itemInHand_ContainerIndex;
        hotBar_ContainerIndex = config.hotBar_ContainerIndex;

        foreach(var c in config.containers)
        {
            containers.TryAdd(c.index,c);
        }
    }

    public ContainerData GetContainer(int index)
    {
        if(containers.TryGetValue(index,out ContainerData containerData))
            return containerData;
        return null;
    }
    public string GetContainerName(int index)
    {
        var cont = GetContainer(index);
        if(cont == null || cont.name.IsEmpty) return defaultContainerName.GetLocalizedString();
        return cont.name.GetLocalizedString();
    }
    public string GetContainerDescription(int index)
    {
        var cont = GetContainer(index);
        if(cont == null || cont.description.IsEmpty) return null;
        return cont.description.GetLocalizedString();
    }
}
