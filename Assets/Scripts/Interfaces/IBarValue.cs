public interface IBarValue 
{
    float GetBarValue();
    float GetCurrentValue();
    void Decrease(float value = 1);
    void SetCurrentValue(float value);
}
