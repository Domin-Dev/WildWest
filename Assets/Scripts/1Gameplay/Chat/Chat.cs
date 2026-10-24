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
    private bool commandIsSelected = false;
    private string[] arguments;
    private Background messageBackground;
    private Background serverMessageBackground;

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
        Print($"[{localTime.ToString("HH:mm:ss")}] {text}").GetComponent<Image>().sprite = serverMessageBackground.Sprite;
    } 
    protected override void SetUp()
    {
        base.SetUp();
        commandList = DebugController.Commands;
        hintParent.gameObject.SetActive(false); 
        textViewport = inputField.textViewport;
        LoadConfig(ChatConfig.Instance);
  
        Instantiate(hintPrefab,hintParent).GetComponent<Image>().sprite = messageBackground.SelectedSprite;
        for(int i = 0; i < displayedHintsCount - 1; i++)
            Instantiate(hintPrefab,hintParent).GetComponent<Image>().sprite = messageBackground.Sprite;
    }
    protected override void TurnOffChat()
    {
        hintParent.gameObject.SetActive(false);
        base.TurnOffChat();
    }
    protected override void ProcessMessage(string message)
    {
        if (!ChatService.TryExecuteLocalCommand(message))
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
        Debug.Log("loading" + config == null);
        messageBackground = UIConfig.GetBackground(config.MessageBackground);
        serverMessageBackground = UIConfig.GetBackground(config.ServerMessageBackground);
    }
    private void UpdateHints(string newValue)
    { 
        if(!ChatService.TrySplitCommand(newValue,out var commandID,out arguments))
        {
            hintParent.gameObject.SetActive(false);
            return;
        }

        var hints = commandList.Where(x => x.Key.StartsWith(commandID)).ToList();
        if(commandList.TryGetValue(commandID,out var variantlist) && (hints.Count == 1 || ChatService.HasWhitespaceBeforeCaret(newValue,inputField.stringPosition)))
        {
            hintParent.GetChild(selectedHintChildIndex).GetComponent<Image>().sprite = messageBackground.Sprite;
            variantlist = variantlist
                .Where(x => x.Arguments.Length >= arguments.Length)
                .OrderBy(x => x.Arguments.Length)
                .OrderByDescending(x => x.CountValidArguments(arguments))
                .ToList();
            commandIsSelected = true; 
            if(variantlist.Count > 0)
                UpdateHintList(variantlist);
            else
                UpdateHintList(new List<CommandBase>());   
        }
        else
        {
            commandIsSelected = false; 
            UpdateHintList(commandList.Where(x => x.Key.StartsWith(commandID)).SelectMany(x => x.Value).ToList());
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
            hintParent.GetChild(newChildIndex).GetComponent<Image>().sprite = messageBackground.SelectedSprite;
            hintParent.GetChild(selectedHintChildIndex).GetComponent<Image>().sprite = messageBackground.Sprite;
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
                var command = hintList[index];
                child.gameObject.SetActive(true);
                bool commandFormatIsEmpty = string.IsNullOrEmpty(command.CommandFormat);
                var text = child.GetComponentInChildren<TextMeshProUGUI>();
                if(commandIsSelected)
                {
                    StringBuilder stringBuilder = new StringBuilder($"/{command.CommandId}");
                    for(int j = 0; j < command.Arguments.Length;j++)
                    {
                        if(arguments?.Length > j)
                        {
                            if(command.ValidateArgument(arguments[j],j))
                                stringBuilder.Append(($" [{command.Arguments[j].name}]").GetColorfulString(ChatConfig.Instance.CorrectColor));
                            else
                                stringBuilder.Append(($" [{command.Arguments[j].name}]").GetColorfulString(ChatConfig.Instance.WarningColor));
                        }
                        else if(arguments?.Length == j)
                            stringBuilder.Append(($" [{command.Arguments[j].name}]").GetColorfulString(ChatConfig.Instance.HighlightColor));
                        else
                            stringBuilder.Append($" [{command.Arguments[j].name}]");
                    }
                    text.text = stringBuilder.ToString();
                }
                else
                    text.text = $"/{command.CommandId.GetColorfulString(ChatConfig.Instance.HighlightColor)}{(commandFormatIsEmpty ? "" : " ")}{command.CommandFormat}";
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
        if(!commandIsSelected)
            ChangeSelectedHint(++selectedHint);
    }
    private void PreviousSuggestion(InputAction.CallbackContext context)
    {
        if(!commandIsSelected)
            ChangeSelectedHint(--selectedHint);
    }
    private void ApplySuggestion(InputAction.CallbackContext context)
    {
        if(commandIsSelected || hintList == null || hintList.Count == 0) return;

        var selectedCommand = hintList[selectedHint].CommandId;
        if(ChatService.TrySplitCommand(inputField.text,out var commandID,out var arguments))
        {
            Match match = Regex.Match(inputField.text, @"\S*");
            if (match.Success)
            {
                inputField.text = inputField.text.Substring(0, match.Index)
                    + $"/{selectedCommand} "
                    + inputField.text.Substring(match.Index + match.Length);
            }
            inputField.MoveTextEnd(false);
            UpdateHints(inputField.text);
        }
    }
    #endregion
}

 