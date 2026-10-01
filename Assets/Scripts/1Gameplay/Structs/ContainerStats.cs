[System.Serializable]
public struct ContainerStats
{
    public bool serverContainer;
    public bool publicContainer;

    
    public int containerIndex;
    public MandatoryProperties mandatoryProperties;
    public int mandatoryData;
    public int capacity;
    public byte waterResistance;
}