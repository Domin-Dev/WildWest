
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public abstract class LerpValue
{
    protected bool isActive;
    public void Update(double currentHour)
    {
        if(isActive)
            Lerp(currentHour);
    }
    protected abstract void Lerp(double currentTime);
}
public class LerpWeather : LerpValue
{    
    private LocalWeather start;
    private LocalWeather target;
    private double startTime;
    private float duration;

    public LerpWeather() {}
    public void Start(LocalWeather startValue,LocalWeather targetValue,double startHour,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startTime = startHour;
        this.duration = duration;
        isActive = true;
    }
    protected override void Lerp(double currentTime)
    {
        double t = currentTime - startTime;
        float progress = (float)t/duration;
        float2 wind = Vector2.Lerp(start.Wind,target.Wind,progress);

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