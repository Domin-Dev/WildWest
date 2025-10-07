
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class LanguageTab : SettingsTab
{

    [SerializeField] private GameObject buttonPrefab;

    private string selected;
    private Dictionary<string, GameObject> languages = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        SetUp();
        SetSettings(MainSettingsManager.instance.settings);
    }
    public override void ResetToDefault()
    {
        MainSettingsManager.instance.languageSettings.SetDefaultSettings();
        SetSettings(MainSettingsManager.instance.settings);
    }
    public override void SaveSettings()
    {
        MainSettingsManager.instance.Save();
    }

    
    private void SetUp()
    {
        if (languages.Count > 0) return;
        foreach (var language in LocalizationSettings.AvailableLocales.Locales)
        {
            string code = language.Identifier.Code;
            GameObject button = Instantiate(buttonPrefab, transform);
            button.GetComponent<Button>().onClick.AddListener(() => SetLanguage(code));
            button.GetComponentInChildren<TextMeshProUGUI>().text =
                $"{language.Identifier.CultureInfo.DisplayName} [ {language.Identifier.CultureInfo.NativeName} ]";
            languages.Add(code,button);
            Debug.Log(code + " !!");
        }
    }
    private void SetSettings(SettingsData settings)
    {
        SetLanguage(settings.language);
    }
    private void SetLanguage(string code)
    {
        SetUp();
        if (!string.IsNullOrEmpty(selected))
        {
           UIAssetsManager.instance.ChangeButtonToWood(languages[selected].GetComponent<Button>());
        }
        selected = code;
        UIAssetsManager.instance.ChangeButtonToIron(languages[selected].GetComponent<Button>());
        MainSettingsManager.instance.languageSettings.SetLanguage(code);
    }
}


