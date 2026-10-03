using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class KeyboardStarter : MonoBehaviour
{
    [SerializeField] private GameObject keyboardprefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    
    public UnityEvent<string> onValueChanged;
    private Keyboard keyboard;

    private void OnEnable()
    {
        this.GetComponent<TMP_InputField>().onSubmit.AddListener(OpenKeyboard);
    }

    private void OnDisable()
    {
        this.GetComponent<TMP_InputField>().onSubmit.RemoveListener(OpenKeyboard);
    }
    private void OpenKeyboard(string value)
    {
        if (!DisableMouseInputSystem.mouseEnabled)
        {
            WindowsManager.instance.SwitchBackground(true);
            keyboard = Instantiate(keyboardprefab, canvas.transform).GetComponent<Keyboard>();
            keyboard.onSaveChanges += Save;
            if (canvasGroup != null) canvasGroup.interactable = false;
        }
    }

    private void Save(string value) 
    {
        keyboard.onSaveChanges -= Save;
        Destroy(keyboard.gameObject);
        WindowsManager.instance.SwitchBackground(false);
        if (canvasGroup != null) canvasGroup.interactable = true;
        WindowsManager.instance.RefreshSelection();
    }
}
