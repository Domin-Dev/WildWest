
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public abstract class LerpValue
{
    protected bool isActive;
    public void Update(float currentHour)
    {
        if(isActive)
            Lerp(currentHour);
    }
    protected abstract void Lerp(float currentHour);
}

public class LerpWeather : LerpValue
{    
    private LocalWeather start;
    private LocalWeather target;
    private float startHour;
    private float duration;

    public LerpWeather() {}
    public void Start(LocalWeather startValue,LocalWeather targetValue,float startHour,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startHour = startHour;
        this.duration = duration;
        isActive = true;
    }
    protected override void Lerp(float currentHour)
    {
        float t = currentHour < startHour ? 24f + currentHour - startHour : currentHour - startHour;
        float progress = t/duration;
        float2 wind = Vector2.Lerp(start.Wind,target.Wind,progress);

        Debug.Log(wind + " test");
        Shader.SetGlobalVector("_WindDirection",new Vector4(wind.x,wind.y,0,0));
        Shader.SetGlobalVector("_DistortWindDirection",0.8f * new Vector4(wind.x,wind.y,0,0));

        if(progress >= 1f)
            isActive = false;    
    }
} 
public class LerpLight : LerpValue
{
    private Color start;
    private Color target;
    private float startHour;
    private float duration;
    private Light2D light2D;

    public LerpLight(Light2D light2D)
    {
        this.light2D = light2D;
    }
    public void Start(Color startValue,Color targetValue,float startHour,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startHour = startHour;
        this.duration = duration;
        isActive = true;
    }
    protected override void Lerp(float currentHour)
    {
        float t = currentHour < startHour ? 24f + currentHour - startHour : currentHour - startHour;
        float progress = t/duration;
        light2D.color = Color.Lerp(start,target,progress);

        if(progress >= 1f)
            isActive = false;    
    }
}