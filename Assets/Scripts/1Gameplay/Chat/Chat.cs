using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Chat : ChatCore<Chat>
{
    #region Fields
    [Header("Hints")]
    [SerializeField] private RectTransform hintParent;    
    [SerializeField] private GameObject hintPrefab;
    [SerializeField] [Min(1)] private int displayedHintsCount = 5;
    [Header("Hints Input")]
    [SerializeField] private InputActionReference nextSuggestion;
    [SerializeField] private InputActionReference previousSuggestion;
    [SerializeField] private InputActionReference applySuggestion;

    private RectTransform textViewport;
    private List<string> history = new List<string>();
    private int LastHistory;
    private int currentLastHistory = -1;
    private const int maxHistory = 10;
    private Dictionary<string,List<CommandBase>> commandList;
    private List<CommandBase> hintList;
    private int selectedHint = 0;
    private int selectedHintChildIndex = 0; 

    #endregion
    #region UnityFuncs
    protected override void OnEnable()
    {
        base.OnEnable();
        inputField.onValueChanged.AddListener(UpdateHints);
        nextSuggestion.action.performed += NextSuggestion;
        previousSuggestion.action.performed += PreviousSuggestion;
        applySuggestion.action.performed += ApplySuggestion;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        inputField.onValueChanged.RemoveListener(UpdateHints);
        nextSuggestion.action.performed -= NextSuggestion;
        previousSuggestion.action.performed -= PreviousSuggestion;
        applySuggestion.action.performed -= ApplySuggestion;
    }
    #endregion
    #region Funcs
    public void PrintPlayerMessage(long time,string text, string player)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(time);
        DateTime localTime = date.ToLocalTime().DateTime;
        Print($"[{localTime.ToString("HH:mm:ss")}] {player.GetColorfulString(ChatConfig.Instance.HighlightColor)}: <noparse>{text}</noparse>");
    }
    public void PrintServerMessage(long time, string text)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(time);
        DateTime localTime = date.ToLocalTime().DateTime;
        Print($"[{localTime.ToString("HH:mm:ss")}] {text}").GetComponent<Image>().sprite = UIConfig.GetBackgroundSprite(Backgrounds.BackgroundType.Iron);
    } 
    protected override void SetUp()
    {
        base.SetUp();
        commandList = DebugController.Commands;
        hintParent.gameObject.SetActive(false); 
        textViewport = inputField.textViewport;
        
        var sprite = UIConfig.GetBackgroundSprite(Backgrounds.BackgroundType.Wood);
        Instantiate(hintPrefab,hintParent).GetComponent<Image>().sprite =  UIConfig.GetBackground(Backgrounds.BackgroundType.Wood).SelectedSprite;
        for(int i = 0; i < displayedHintsCount - 1; i++)
            Instantiate(hintPrefab,hintParent).GetComponent<Image>().sprite = sprite;

        LoadConfig(ChatConfig.Instance);
    }
    protected override void TurnOffChat()
    {
        hintParent.gameObject.SetActive(false);
        base.TurnOffChat();
    }
    protected override void ProcessMessage(string message)
    {
        if (!ChatService.IsCommand(message) || !ChatService.TryExecuteLocalCommand(message))
        {
            ChatService.ClientSendsMessage(inputField.text);
            SaveToHistory();
        }
    }
    private void SaveToHistory()
    {
        if (history.Count < maxHistory)
        {
            history.Add(inputField.text);
            LastHistory = history.Count - 1;
        }
        else
        {
            LastHistory++;
            if (LastHistory >= maxHistory)
            {
                LastHistory = 0;
            }

            history[LastHistory] = inputField.text;
        }
    } 
    private Vector3 GetCaretWorldPosition()
    {
        TMP_Text text = inputField.textComponent;
        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;
        int count = textInfo.characterCount;
        int caret = Mathf.Clamp(inputField.stringPosition, 0, inputField.text.Length);

        if (count == 0)
            return text.transform.TransformPoint(text.rectTransform.rect.min);;

        if (caret >= count)
        {
            TMP_CharacterInfo last = textInfo.characterInfo[count - 1];
            return text.transform.TransformPoint(last.bottomRight);
        }

        TMP_CharacterInfo current = textInfo.characterInfo[caret];

        return text.transform.TransformPoint(current.bottomLeft);
    }
    private void LoadConfig(ChatConfig config)
    {
        
    }
    private void UpdateHints(string newValue)
    {
        Debug.Log(newValue + " koko");  
        if(!ChatService.IsCommand(newValue))
        {
            hintParent.gameObject.SetActive(false);
            return;
        }
        string[] command = newValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandID = command[0].Substring(1).ToLower();

        if(commandList.TryGetValue(commandID,out var variantlist))
        {
            UpdateHintList(variantlist);
        }
        else
        {
            var hints = commandList.Where(x => x.Key.StartsWith(commandID)).SelectMany(x => x.Value).ToList();
            UpdateHintList(hints);
        }

        if(hintList.Count > 0)
        {
            hintParent.gameObject.SetActive(true);
            var position = GetCaretWorldPosition() + new Vector3(0,textViewport.rect.height - 12f);
            hintParent.position = position;
        }
        else
            hintParent.gameObject.SetActive(false);
    }
    private void UpdateHintList(List<CommandBase> hints)
    {
        if((hintList == null || hintList.Count == 0) && (hints == null || hints.Count == 0)) return;
        hintList = hints;
        ChangeSelectedHint(0);
    }
    private void ChangeSelectedHint(int newValue)
    {
        if(hintList == null || hintList.Count == 0)
            return; 
        selectedHint = math.clamp(newValue,0,hintList.Count-1);
        int newChildIndex = math.max(0,displayedHintsCount - hintList.Count + selectedHint);    
        if(newChildIndex != selectedHintChildIndex) 
        {   
            hintParent.GetChild(newChildIndex).GetComponent<Image>().sprite = UIConfig.GetBackground(Backgrounds.BackgroundType.Wood).SelectedSprite;
            hintParent.GetChild(selectedHintChildIndex).GetComponent<Image>().sprite = UIConfig.GetBackgroundSprite(Backgrounds.BackgroundType.Wood);
            selectedHintChildIndex = newChildIndex;
        }
        UpdateHints(); 
    }   
    private void UpdateHints()
    {
        int startIndex = selectedHint - selectedHintChildIndex;
        for(int i = 0; i < hintParent.childCount; i++)
        {
            var child = hintParent.GetChild(i);
            int index = startIndex + i;
            if(index >= 0 && index < hintList.Count)
            {
                Debug.Log(index);
                var command = hintList[index];
                child.gameObject.SetActive(true);
                bool commandFormatIsEmpty = string.IsNullOrEmpty(command.commandFormat);
                child.GetComponentInChildren<TextMeshProUGUI>().text = $"/{command.commandId.GetColorfulString(ChatConfig.Instance.HighlightColor)}{(commandFormatIsEmpty ? "" : " ")}{command.commandFormat}";
            }
            else
                child.gameObject.SetActive(false);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(hintParent);
    }
    #endregion
    #region InputActions
    private void NextSuggestion(InputAction.CallbackContext context)
    {
        ChangeSelectedHint(++selectedHint);
    }
    private void PreviousSuggestion(InputAction.CallbackContext context)
    {
        ChangeSelectedHint(--selectedHint);
    }
    private void ApplySuggestion(InputAction.CallbackContext context)
    {
        if(hintList == null || hintList.Count == 0) return;
        if(inputField.text.Contains('/'))
        {
            inputField.text = Regex.Replace(inputField.text, @"/\S*","/" + hintList[selectedHint].commandId);
            inputField.MoveTextEnd(false);
            UpdateHints(inputField.text);
        }
    }
    #endregion
}

 