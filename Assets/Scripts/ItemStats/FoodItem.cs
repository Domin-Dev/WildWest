using Unity.Mathematics;

public class FoodItem : ItemStats, IBarValue
{
    public float maxLifePonits { private set; get; }
    public float currentLifePoints { private set; get; }

    public FoodItem(int itemID, int maxLifePoints, int itemCount = 1) : base(itemID, itemCount)
    {
        this.maxLifePonits = maxLifePoints;
        currentLifePoints = this.maxLifePonits;
    }
    public FoodItem(int itemID, int itemCount, int maxLifePoints, int currentLifePoints) : base(itemID, itemCount)
    {
        this.maxLifePonits = maxLifePoints;
        this.currentLifePoints = currentLifePoints;
    }
    public FoodItem(FoodItem item) : base(item)
    {
        this.maxLifePonits = item.maxLifePonits;
        this.currentLifePoints = item.currentLifePoints;
    }
    public float GetBarValue()
    {
        return currentLifePoints / (float)maxLifePonits;
    }

    public override ItemStats Clon()
    {
        return new FoodItem(this);
    }

    public void Decrease(float value = 1)
    {
        currentLifePoints = math.clamp(currentLifePoints - value, 0, maxLifePonits);
    }
}