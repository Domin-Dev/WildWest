using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;


[System.Serializable]
public class ContainerData
{
    [SerializeField] private ContainerStats _containerStats;
    [SerializeField] private LocalizedString  _containerName;
    [SerializeField] private LocalizedString  _description;

    public int index => _containerStats.containerIndex;
    public ContainerStats stats => _containerStats;
    public LocalizedString name => _containerName;
    public LocalizedString description => _description;
}


[CreateAssetMenu(fileName = "EquipmentConfig", menuName = "GameAsset/ConfigFiles/EquipmentConfig")]
public class EquipmentConfig : Config<EquipmentConfig>
{
    public List<ContainerData> containers;
    private Dictionary<int,ContainerData> containersMap;
    public LocalizedString defaultContainerName;
    public int itemInHand_ContainerIndex;
    public SlotPosition itemInHand_SlotPosition => new SlotPosition(itemInHand_ContainerIndex,0);
    public int hotBar_ContainerIndex;
    public int start_ContainerIndex;
    public int chunkItems_ContainerIndex;

    protected override void Start()
    {
        containersMap = new Dictionary<int, ContainerData>();
        foreach(var c in containers)
            containersMap.TryAdd(c.index,c);
    }
    public static ContainerData GetContainer(int index)
    {
        if(Instance.containersMap.TryGetValue(index,out ContainerData containerData))
            return containerData;
        return null;
    }
    public static string GetContainerName(int index)
    {
        var cont = GetContainer(index);
        if(cont == null || cont.name.IsEmpty) return Instance.defaultContainerName.GetLocalizedString();
        return cont.name.GetLocalizedString();
    }
    public static string GetContainerDescription(int index)
    {
        var cont = GetContainer(index);
        if(cont == null || cont.description.IsEmpty) return null;
        return cont.description.GetLocalizedString();
    }
}
 