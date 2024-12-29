
using Unity.Mathematics;

public class LiquidContainerItem : ItemStats, IBarValue
{
    public float maxCapacity { private set; get; }
    public float currentFill { private set; get; }

    public LiquidContainerItem(int itemID, float maxCapacity, float fill = 0) : base(itemID)
    {
        this.maxCapacity = maxCapacity;
        this.currentFill = math.clamp(fill,0,maxCapacity);
    }
    public LiquidContainerItem(LiquidContainerItem item) : base(item)
    {
        this.maxCapacity = item.maxCapacity;
        this.currentFill = item.currentFill;
    }

    public override ItemStats Clon()
    {
        return new LiquidContainerItem(this);
    }

    public float GetBarValue()
    {
        return currentFill / maxCapacity;
    }

    public void Decrease(float value = 1)
    {
        currentFill = math.clamp(currentFill- value, 0,maxCapacity);
    }
}