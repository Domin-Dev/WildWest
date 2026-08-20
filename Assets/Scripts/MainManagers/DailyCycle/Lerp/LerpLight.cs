using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LerpLight : LerpValue
{
    private Color start;
    private Color target;
    private double startTime;
    private float duration;
    private Light2D light2D;

    public LerpLight(Light2D light2D)
    {
        this.light2D = light2D;
    }
    public void Start(Color startValue,Color targetValue,double startTime,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startTime = startTime;
        this.duration = duration;
        isActive = true;
    }

    public void Set(Color value)
    {
        light2D.color = value;
    }
    protected override void Lerp(double currentTime)
    {
        double t =  currentTime - startTime;
        float progress = (float)t/duration;
        light2D.color = Color.Lerp(start,target,progress);

        if(progress >= 1f)
            isActive = false;    
    }
}
