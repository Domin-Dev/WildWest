using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    private bool active = false;
    public static LocaleManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        int id = PlayerPrefs.GetInt("LocaleID", 0);
        ChangeLocale(id);
        DontDestroyOnLoad(gameObject);
    }
    public void ChangeLocale(int index)
    {
        if (active)
            return;
        StartCoroutine(SetLocale((index)));
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
            PlayerPrefs.SetInt("LocaleID", index);
        }
        active = false;
    }
}
