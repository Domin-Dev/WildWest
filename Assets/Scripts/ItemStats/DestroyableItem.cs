using Unity.Mathematics;

public class DestroyableItem : ItemStats, IBarValue
{
    public float maxLifePonits { private set; get; }
    public float currentLifePoints { private set; get; }

    public DestroyableItem(int itemID, float maxLifePoints, int itemCount = 1) : base(itemID, itemCount)
    {
        this.maxLifePonits = maxLifePoints;
        currentLifePoints = this.maxLifePonits;
    }

    public DestroyableItem(InventorySlot slot,ItemBarData barData) : base(slot.itemId, 1)
    {
        this.maxLifePonits = barData.maxValue;
        this.currentLifePoints = barData.value;
    }

    public DestroyableItem(int itemID, int itemCount, float maxLifePoints, float currentLifePoints) : base(itemID, itemCount)
    {
        this.maxLifePonits = maxLifePoints;
        this.currentLifePoints = currentLifePoints;
    }
    public DestroyableItem(DestroyableItem item) : base(item)
    {
        this.maxLifePonits = item.maxLifePonits;
        this.currentLifePoints = item.currentLifePoints;
    }

    public DestroyableItem(DestroyableItem item, int quantity) : base(item,quantity)
    {
        this.maxLifePonits = item.maxLifePonits;
        this.currentLifePoints = item.currentLifePoints;
    }

    public float GetBarValue()
    {
        return currentLifePoints / (float)maxLifePonits;
    }

    public override ItemStats Clon(int quantity)
    {
        return new DestroyableItem(this,quantity);
    }

    public void Decrease(float value = 1)
    {
        currentLifePoints = math.clamp(currentLifePoints - value, 0, maxLifePonits);
    }

    public float GetCurrentValue()
    {
        return currentLifePoints;
    }

    public void SetCurrentValue(float value)
    {
        currentLifePoints = value;
    }
}