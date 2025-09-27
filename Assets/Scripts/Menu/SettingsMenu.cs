using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Button graphicSettings;
    [SerializeField] private Button soundsSettings;
    [SerializeField] private Button controlSettings;
    [SerializeField] private Button languageSettings;
    [SerializeField] private Button credits;

    [SerializeField] private Button back;

    private void Start()
    {
        WindowsManager.instance.SetNewSelectedButton(graphicSettings.gameObject);
        back.onClick.AddListener(CloseSettings);
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
}
