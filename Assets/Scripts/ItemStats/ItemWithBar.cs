using Unity.Mathematics;

public class ItemWithBar : ItemStats, IBarValue
{
    public float maxValue { private set; get; }
    public float current { private set; get; }

    public ItemWithBar(int itemID, float maxLifePoints, int itemCount = 1) : base(itemID, itemCount)
    {
        this.maxValue = maxLifePoints;
        current = this.maxValue;
    }

    public ItemWithBar(InventorySlot slot,ItemBarData barData) : base(slot)
    {
        this.maxValue = barData.maxValue;
        this.current = barData.value;
    }

    public ItemWithBar(int itemID, int itemCount, float maxLifePoints, float currentLifePoints) : base(itemID, itemCount)
    {
        this.maxValue = maxLifePoints;
        this.current = currentLifePoints;
    }
    public ItemWithBar(ItemWithBar item) : base(item)
    {
        this.maxValue = item.maxValue;
        this.current = item.current;
    }

    public ItemWithBar(ItemWithBar item, int quantity) : base(item,quantity)
    {
        this.maxValue = item.maxValue;
        this.current = item.current;
    }

    public float GetBarValue()
    {
        return current / (float)maxValue;
    }

    public override ItemStats Clon(int quantity)
    {
        return new ItemWithBar(this,quantity);
    }

    public override ItemStats Clon()
    {
        return new ItemWithBar(this);
    }


    public void Decrease(float value = 1)
    {
        current = math.clamp(current - value, 0, maxValue);
    }

    public float GetCurrentValue()
    {
        return current;
    }

    public void SetCurrentValue(float value)
    {
        current = value;
    }
}