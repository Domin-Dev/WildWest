using System;
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocaleSelector : MonoBehaviour
{
    [SerializeField] private Switch localeSwitch;

    private void Start()
    {
        localeSwitch.SetUpSwitch(0, LocalizationSettings.AvailableLocales.Locales.Count,"",false,false);
    }

    private void OnEnable()
    {
        localeSwitch.OnChangedValue += LocaleManager.instance.ChangeLocale;
    }

    private void OnDisable()
    {
        localeSwitch.OnChangedValue -= LocaleManager.instance.ChangeLocale;
    }
}
