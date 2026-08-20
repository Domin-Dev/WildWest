
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