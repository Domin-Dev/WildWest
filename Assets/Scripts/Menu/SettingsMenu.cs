using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button videoSettings;
    [SerializeField] private Button soundsSettings;
    [SerializeField] private Button controlSettings;
    [SerializeField] private Button languageSettings;
    [SerializeField] private Button credits;

    [SerializeField] private Button back;
    [Header("Tabs")]
    [SerializeField] private GameObject mainTab;
    [SerializeField] private GameObject videoTab;

    private void Start()
    {
        WindowsManager.instance.SetNewSelectedButton(videoSettings.gameObject);
        back.onClick.AddListener(CloseSettings);

        videoSettings.onClick.AddListener(OpenVideoSettings);
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
    }

}
