using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class EquipmentConfig 
{
    private static EquipmentConfig i;
    private static Dictionary<int,ContainerData> containers;



    public static LocalizedString defaultContainerName {private set;get;}
    public IReadOnlyDictionary<int, ContainerData> Containers => containers;
    public readonly static int itemInHand_ContainerIndex;
    public static SlotPosition itemInHand_SlotPosition => new SlotPosition(itemInHand_ContainerIndex,0);

    public readonly static int hotBar_ContainerIndex;
    public readonly static int start_ContainerIndex;



    public static EquipmentConfig Instance
    {
        get
        {
            if (i == null)
            {
                i = new EquipmentConfig();
            }
            return i;
        }
    }
    
    static EquipmentConfig()
    {
        var config = Resources.Load<EquipmentConfigData>("Config/EquipmentConfig");
        defaultContainerName = config.defaultContainerName;
        containers = new Dictionary<int, ContainerData>();

        itemInHand_ContainerIndex = config.itemInHand_ContainerIndex;
        hotBar_ContainerIndex = config.hotBar_ContainerIndex;
        start_ContainerIndex = config.start_ContainerIndex;

        foreach(var c in config.containers)
        {
            containers.TryAdd(c.index,c);
        }
    }

    public static ContainerData GetContainer(int index)
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
