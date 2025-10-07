using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public abstract class Settings
{
    protected SettingsData settingsData;
    public virtual void SetUp(SettingsData data, bool defaultSettings)
    {
        this.settingsData = data;
        if (defaultSettings)
            SetDefaultSettings();
        else
            SetSettings();
    }
    public abstract void SetDefaultSettings();
    public abstract void SetSettings();
}

