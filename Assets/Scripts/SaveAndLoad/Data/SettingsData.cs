using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;


[System.Serializable]
public class SettingsData
{
    public int resolutionWidth;
    public int resolutionHeight;
    public bool fullScreen;

    public int fpsLimit;
    public int fontIndex;
}