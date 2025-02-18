using Unity.Mathematics;

public class FoodItem : ItemStats, IBarValue , IStackingBarValues
{
    public float maxShelfLife { private set; get; }
    public float currentShelfLife { private set; get; }

    public FoodItem(int itemID, int maxLifePoints, int itemCount = 1) : base(itemID, itemCount)
    {
        this.maxShelfLife = maxLifePoints;
        currentShelfLife = this.maxShelfLife;
    }

    public FoodItem(FoodItem item) : base(item)
    {
        this.maxShelfLife = item.maxShelfLife;
        this.currentShelfLife = item.currentShelfLife;
    }
    public float GetBarValue()
    {
        return currentShelfLife / (float)maxShelfLife;
    }


    public override ItemStats Clon()
    {
        return new FoodItem(this);
    }
    public void Decrease(float value = 1)
    {
        currentShelfLife = math.clamp(currentShelfLife - value, 0, maxShelfLife);
    }

    public void SetCurrentValue(float value)
    {
        this.currentShelfLife = math.clamp(value, 0, maxShelfLife); ;
    }

    public float GetCurrentValue()
    {
        return currentShelfLife;
    }

    public void Stacking(int number, float value)
    {
        float newShelfLife = itemCount * currentShelfLife;
        newShelfLife += number * value;
        SetCurrentValue(newShelfLife/(number + itemCount));
    }
}