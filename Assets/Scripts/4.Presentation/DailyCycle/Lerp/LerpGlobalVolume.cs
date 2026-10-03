using UnityEngine;
using UnityEngine.Rendering;

public class LerpGlobalVolume: LerpValue
{
    private WhiteBalance start;
    private WhiteBalance target;
    private double startTime;
    private float duration;

    private Volume volume;
    private UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;

    public LerpGlobalVolume(Volume volume)
    {
        this.volume = volume;
        volume.profile.TryGet(out whiteBalance);
    }
    public void Start(WhiteBalance startValue,WhiteBalance targetValue,double startTime,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startTime = startTime;
        this.duration = duration;
        isActive = true;
    }

    public void Set(WhiteBalance value)
    {
        whiteBalance.temperature.value = value.Temperature;
        whiteBalance.tint.value = value.Tint;
    }
    protected override void Lerp(double currentTime)
    {
        double t = currentTime - startTime;
        float progress = (float)t/duration;

        whiteBalance.temperature.value = Mathf.Lerp(start.Temperature,target.Temperature,progress);
        whiteBalance.tint.value = Mathf.Lerp(start.Tint,target.Tint,progress);

        if(progress >= 1f)
            isActive = false;    
    }

    
}