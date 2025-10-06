using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Button back;
    [SerializeField] private Button reset;
    [Header("Tabs")]
    [SerializeField] private List<SettingsTab> tabs;
    [SerializeField] private GameObject startSelectedButton;
    [SerializeField] private SettingsTab startTab;

    private SettingsTab currentTab;
    private SettingsData currentSettings;

    private void Start()
    {
        WindowsManager.instance.SetNewSelectedButton(startSelectedButton.gameObject);
        foreach (var tab in tabs) 
            SetUpTab(tab);
        currentSettings = LoadSystem.LoadSettings();
        if(currentSettings == null)
        {

        }
        OpenTab(startTab);
    }
    private void OnDestroy()
    {
        MenuManager.instance.SwitchMenuButtons(true);
    }
    private void SetUpTab(SettingsTab settingsTab)
    {
        settingsTab.startButton?.onClick.AddListener(() => OpenTab(settingsTab));
        settingsTab.gameObject.SetActive(false);
    }


    private void CloseSettings()
    {
        WindowsManager.instance.UnloadScene(12);
        MenuManager.instance?.SetSelectedButton();
        if (GameInfo.instance.lastLoadedScene == -1)
            WindowsManager.instance.SwitchBackground(false);
        else
            WindowsManager.instance.LoadScene(GameInfo.instance.lastLoadedScene);
    }
    private void CloseTabs()
    {
        if (currentTab != null)
        {
            currentTab.SaveSettings();
            currentTab.gameObject.SetActive(false);
        }
    }
    private void OpenTab(SettingsTab settingsTab)
    {
        CloseTabs();
        currentTab = settingsTab;
        settingsTab.gameObject.SetActive(true);
        back.onClick.RemoveAllListeners();
        reset.onClick.RemoveAllListeners();

        if (currentTab == startTab)
        {
            back.onClick.AddListener(CloseSettings);
            reset.onClick.AddListener(ResetAllSettings);
        }
        else
        {
            reset.onClick.AddListener(currentTab.ResetToDefault);
            back.onClick.AddListener(() => OpenTab(startTab));
        }
    }

    private void ResetAllSettings()
    {
        foreach (var tab in tabs)
        {
            tab.ResetToDefault();
        }
    }
}
