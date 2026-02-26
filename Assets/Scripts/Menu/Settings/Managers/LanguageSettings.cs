using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;


public class LanguageSettings : Settings
{
    private LocaleManager localeManager;
    public void SetUp(LocaleManager localeManager, SettingsData data,bool defaultSettings)
    {
        this.localeManager = localeManager;    
        base.SetUp(data, defaultSettings);
    }
    public override void SetSettings()
    {
        SetLanguage(settingsData.language);
    }
    public override void SetDefaultSettings()
    {
        SetLanguage("en");
    }

    #region Set Value

    public void SetLanguage(string code)
    {
        localeManager.ChangeLocale(code);
        settingsData.language = code;
    }

    #endregion
}

