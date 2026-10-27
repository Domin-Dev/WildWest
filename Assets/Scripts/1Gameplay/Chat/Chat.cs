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
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;


public class HintList
{
    private int selectedHint;
    private int displayedHintsCount;
    private List<CommandBase> commands;

    public HintList(int displayedHintsCount)
    {
        this.displayedHintsCount = displayedHintsCount;
    }
    public void InputChanged(string newValue)
    {
        
    }
}

public class Chat : ChatCore<Chat>
{
    #region Fields
    [Header("Hints")]
    [SerializeField] private RectTransform hintParent;    
    [SerializeField] private GameObject hintPrefab;
    [SerializeField] private TextMeshProUGUI placeholder;
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
    private Dictionary<string,CommandBase> commandList;
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
        (inputField as CustomInputField).OnCaretPositionChanged += CaretPositionChanged;
        nextSuggestion.action.performed += NextSuggestion;
        previousSuggestion.action.performed += PreviousSuggestion;
        applySuggestion.action.performed += ApplySuggestion;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        inputField.onValueChanged.RemoveListener(UpdateHints);
        (inputField as CustomInputField).OnCaretPositionChanged -= CaretPositionChanged;
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
        placeholder.text = default;
        if(!ChatService.TrySplitCommand(newValue,out var commandID,out arguments))
        {
            hintParent.gameObject.SetActive(false);
            return;
        }

        var hints = commandList.Where(x => x.Key.StartsWith(commandID)).Select(x => x.Value).ToList();
        if(commandList.TryGetValue(commandID,out var command) && (hints.Count == 1 || ChatService.HasWhitespaceBeforeCaret(newValue,inputField.stringPosition)))
        {
            hintParent.GetChild(selectedHintChildIndex).GetComponent<Image>().sprite = messageBackground.Sprite;
            commandIsSelected = true; 
            UpdateHintList(new List<CommandBase>(){command});  
        }
        else
        {   
            commandIsSelected = false; 
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
        if(!commandIsSelected && newChildIndex != selectedHintChildIndex) 
        {   
            hintParent.GetChild(newChildIndex).GetComponent<Image>().sprite = messageBackground.SelectedSprite;
            hintParent.GetChild(selectedHintChildIndex).GetComponent<Image>().sprite = messageBackground.Sprite;
            selectedHintChildIndex = newChildIndex;
        }
        UpdateHints(); 
    }   
    private void CaretPositionChanged()
    {
        if(hintList == null || hintList.Count == 0)
            return;
        UpdateHints();
    }
    private void UpdateHints()
    {
        int startIndex = selectedHint - selectedHintChildIndex;
        int argumentIndex = ChatService.GetArgumentIndex(inputField.text,inputField.stringPosition);

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
                    StringBuilder placeHolderBuilder = new StringBuilder();
                    for(int j = 0; j < command.RequiredArguments.Length;j++)
                    {
                        if(j > argumentIndex)
                        {
                            if(arguments?.Length > j)
                            {
                                var color = command.ValidateArgument(arguments[j],j) ? ChatConfig.Instance.CorrectColor : ChatConfig.Instance.WarningColor;
                                placeHolderBuilder.Append(($" [{command.GetArgument(j).Name}]").GetColorfulString(color));                            }
                            else
                                placeHolderBuilder.Append($" ({command.GetArgument(j).Name})");
                        }
                        else if(j == argumentIndex)
                        {
                            if(arguments?.Length > j)
                            {
                                var color = command.ValidateArgument(arguments[j],j) ? ChatConfig.Instance.CorrectColor : ChatConfig.Instance.WarningColor;
                                placeHolderBuilder.Append(($" [{command.GetArgument(j).Name.GetColorfulString(ChatConfig.Instance.HighlightColor)}]").GetColorfulString(color));
                            }
                            else
                                placeHolderBuilder.Append(($" [{command.GetArgument(j).Name}]").GetColorfulString(ChatConfig.Instance.HighlightColor));     
                        }
 
                        if(arguments?.Length > j)
                        {
                            var color = command.ValidateArgument(arguments[j],j) ? ChatConfig.Instance.CorrectColor : ChatConfig.Instance.WarningColor;
                            if(argumentIndex == j)
                                stringBuilder.Append(($" [{command.GetArgument(j).Name.GetColorfulString(ChatConfig.Instance.HighlightColor)}]").GetColorfulString(color));
                            else
                                stringBuilder.Append(($" [{command.GetArgument(j).Name}]").GetColorfulString(color));
                        }
                        else if(argumentIndex == j)
                            stringBuilder.Append(($" [{command.GetArgument(j).Name}]").GetColorfulString(ChatConfig.Instance.HighlightColor));             
                        else
                            stringBuilder.Append($" [{command.GetArgument(j).Name}]");
                    }
                    


                    string input = inputField.text.Trim() + (char.IsWhiteSpace(inputField.text[inputField.text.Length-1]) ? " " : default);
                    placeHolderBuilder.Insert(0,$"{input}");
                    placeholder.text = placeHolderBuilder.ToString();
                    text.text = stringBuilder.ToString();

                 //   Debug.Log(index + " " + inputField.text + " " + inputField.stringPosition);
                    inputField.SetTextWithoutNotify(input + new string(' ',  placeholder.textInfo.characterCount*2));
                    break;
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
        if(commandIsSelected)
            GetNextArgument();
        else
            ChangeSelectedHint(++selectedHint);
    }
    private void PreviousSuggestion(InputAction.CallbackContext context)
    {
        if(commandIsSelected)
            GetPreviousArgument();
        else
            ChangeSelectedHint(--selectedHint);
    }
    private void ApplySuggestion(InputAction.CallbackContext context)
    {
        if(hintList == null || hintList.Count == 0) return;        
        if(commandIsSelected)
            CompleteArgument();
        else
        {         
            Match match = Regex.Match(inputField.text, @"\S*");
            if (match.Success)
            {
                inputField.text = inputField.text.Substring(0, match.Index)
                    + $"/{hintList[selectedHint].CommandId} "
                    + inputField.text.Substring(match.Index + match.Length);
            }
            inputField.MoveTextEnd(false);
            UpdateHints(inputField.text);          
        }
    }  
    private void CompleteArgument()
    {
        if(ChatService.TrySplitCommand(inputField.text,out var commandID,out var arguments) && arguments != null)
        {           
            int argumentIndex = ChatService.GetArgumentIndex(inputField.text,inputField.stringPosition);
            if(argumentIndex < 0 ) return;
            bool argIsNotEmpty = argumentIndex < arguments.Length;
            string arg = argIsNotEmpty ? arguments[argumentIndex] : default; 
        
            for(int k = 0; k < hintList.Count; k++)
            {
                var command = hintList[k];
                if(command.RequiredArguments.Length > argumentIndex && command.TryCompleteArgument(arg,argumentIndex,out var newValue))
                {
                    if(argIsNotEmpty)
                        arguments[argumentIndex] = newValue;
                    else
                    {
                        Array.Resize(ref arguments, arguments.Length + 1);
                        arguments[arguments.Length - 1] = newValue;
                    }

                    inputField.text = "/" + commandID + " " + string.Join(" ", arguments);
                    int pos = 1 + commandID.Length + 1;           
                    for (int i = 0; i <= argumentIndex; i++)
                    {
                        pos += arguments[i].Length;
                        if (i < argumentIndex) pos++;             
                    }
                    inputField.stringPosition = pos;
                    break;
                }
            }
        }
    }  
    private void GetNextArgument()
    {
        if(ChatService.TrySplitCommand(inputField.text,out var commandID,out var arguments) && arguments != null)
        {           
            int argumentIndex = ChatService.GetArgumentIndex(inputField.text,inputField.stringPosition);
            if(argumentIndex < 0 ) return;
            bool argIsNotEmpty = argumentIndex < arguments.Length;
            string arg = argIsNotEmpty ? arguments[argumentIndex] : default; 
        
            for(int k = 0; k < hintList.Count; k++)
            {
                var command = hintList[k];
                if(command.RequiredArguments.Length > argumentIndex 
                && command.TryGetICommandArgument(arg,argumentIndex,out var commandArgument) 
                && commandArgument.TryGetNext(out var newValue))
                {
                    if(argIsNotEmpty)
                        arguments[argumentIndex] = newValue;
                    else
                    {
                        Array.Resize(ref arguments, arguments.Length + 1);
                        arguments[arguments.Length - 1] = newValue;
                    }

                    inputField.text = "/" + commandID + " " + string.Join(" ", arguments);
                    int pos = 1 + commandID.Length + 1;           
                    for (int i = 0; i <= argumentIndex; i++)
                    {
                        pos += arguments[i].Length;
                        if (i < argumentIndex) pos++;             
                    }
                    inputField.stringPosition = pos;
                    break;
                }
            }
        }
    }
    private void GetPreviousArgument()
    {
        
    }
    #endregion
}

 