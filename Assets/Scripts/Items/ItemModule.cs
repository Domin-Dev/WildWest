using UnityEngine;

[System.Serializable]
public abstract class ItemModule
{
    public abstract void Apply(GameObject user);
}





[System.Serializable]
public class DurabilityModule : ItemModule
{
    [SerializeField] public int maxDurability;

    public override void Apply(GameObject user)
    {
        Debug.Log("Using durability module: " + maxDurability);
    }
}



