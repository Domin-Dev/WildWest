using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button videoButton;
    [SerializeField] private Button soundsButton;
    [SerializeField] private Button controlButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button credits;

    [SerializeField] private Button back;
    [SerializeField] private Button reset;
    [Header("Tabs")]
    [SerializeField] private GameObject mainTab;
    [SerializeField] private GameObject videoTab;

    [SerializeField] private VideoSettings video;

    private void Start()
    {
        WindowsManager.instance.SetNewSelectedButton(videoButton.gameObject);
        OpenMainTab();
        videoButton.onClick.AddListener(OpenVideoSettings);
    }

    private void OnDestroy()
    {
        MenuManager.instance.SwitchMenuButtons(true);
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
        mainTab.SetActive(false);
        videoTab.SetActive(false);    
    }
    private void OpenVideoSettings()
    {
        CloseTabs();
        videoTab.SetActive(true);
        back.onClick.RemoveAllListeners();
        back.onClick.AddListener(OpenMainTab);
    }
    private void OpenMainTab()
    {
        CloseTabs();
        mainTab.SetActive(true);
        back.onClick.RemoveAllListeners();
        back.onClick.AddListener(CloseSettings);
    }
}
