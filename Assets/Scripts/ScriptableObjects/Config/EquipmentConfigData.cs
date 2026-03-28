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
public class EquipmentConfigData : ScriptableObject
{
    public List<ContainerData> containers;
    public LocalizedString defaultContainerName;

    public int itemInHand_ContainerIndex;
    public int hotBar_ContainerIndex;
    public int start_ContainerIndex;
}
