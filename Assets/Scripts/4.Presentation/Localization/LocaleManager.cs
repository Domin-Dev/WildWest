using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    private bool active = false;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void ChangeLocale(int index)
    {
        if (active)
            return;
        StartCoroutine(SetLocale((index)));
    }
    public void ChangeLocale(string code)
    {
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            var item = LocalizationSettings.AvailableLocales.Locales[i];
            if (string.Equals(code,item.Identifier.Code))
            {
                ChangeLocale(i);
                return;
            }
        }
    }
    public void ChangeLocale(object s,int index)
    {
        ChangeLocale(index);
    }
    IEnumerator SetLocale(int index)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        if (index < LocalizationSettings.AvailableLocales.Locales.Count && index >= 0)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
        active = false;
    }

    public string[] GetLocaleCodes()
    {
        string[] tab = new string[LocalizationSettings.AvailableLocales.Locales.Count];
        int i = 0;
        foreach(var item in LocalizationSettings.AvailableLocales.Locales)
        {
            tab[i] = item.Identifier.Code;
            i++;
        }
        return tab;
    }
}
