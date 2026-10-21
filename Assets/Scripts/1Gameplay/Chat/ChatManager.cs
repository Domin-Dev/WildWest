using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Chat : ChatCore
{
    [Header("Chat GameObjects")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Image background;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private RectTransform hintParent;    

    [Header("Prefabs")]
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject hintPrefab;

    [Header("Hints")]
    [SerializeField] [Min(1)] private int displayedHintsCount;
    //chat
    private Scrollbar chatScrollbar;
    private RectTransform chathandle;
    private Transform content;
    private RectTransform textViewport;

    private int selectedHint;
    private List<string> history = new List<string>();
    private int LastHistory;
    private int currentLastHistory = -1;

    private const int maxHistory = 10;
    private const int maxLog = 30;
    private const int maxMessageOnScreen = 8;
    private const int timeToDisappear = 10;
    public static readonly Color highlightColor = new Color(218f / 255f, 165f / 255f, 32f / 255f);
    private static Color invibsile = new Color(1, 1, 1, 0);

    private bool isTimer = false;
    private  int LastIndex = 0;
    public int[] indexes = new int[maxMessageOnScreen];
    public int[] timers = new int[maxMessageOnScreen];
    //

    private bool isChat = false;
    private Dictionary<string,List<CommandBase>> commandList;
    private List<CommandBase> hintList;
    public static Chat instance { private set; get; }
    public bool isChatting { private set; get; }
    float timer = 0;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
        SetUp();  
    }
    private void Start()
    {
        ClearIndexes();
        commandList = DebugController.Commands;
        hintParent.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);  
    }

    
    // private void OnEnable()
    // {
    //     inputField.onValueChanged.AddListener(UpdateHints);
    // }
    // private void OnDisable()
    // {
    //     inputField.onValueChanged.RemoveListener(UpdateHints);
    // }
    private void Update()
    {     
        // if(isTimer)
        // {
        //     timer += Time.deltaTime;
        //     if(timer >= 1)
        //     {
        //         timer = 0;
        //         UpdateTimers();
        //     }
        // }

        // if (Input.GetKeyDown(KeyCode.T) && !inputField.isFocused && !WindowsManager.instance.HasOpenWidnows())
        // {
        //     SwitchChat();
        // }

        // if(isChat)
        // {
        //     if (Input.GetKeyDown(KeyCode.Return))
        //     {
        //         SendMessage();
        //     }

        //     if (Input.GetKeyDown(KeyCode.Escape))
        //     {
        //         SwitchChat();
        //     }

        //     if (Input.GetKeyDown(KeyCode.UpArrow))
        //     {
        //         currentLastHistory = math.clamp(++currentLastHistory, 0, history.Count - 1);
        //         int index = LastHistory - currentLastHistory;


        //         if (index < 0) index = history.Count + index;
        //         if(index < history.Count) inputField.text = history[index];
        //     }
            
        //     if (Input.GetKeyDown(KeyCode.DownArrow))
        //     {
        //         currentLastHistory = math.clamp(--currentLastHistory, 0, history.Count - 1);
        //         int index = LastHistory - currentLastHistory;

        //         if (index < 0) index = history.Count + index;
        //         if (index < history.Count) inputField.text = history[index];
        //     }
        // }
    }
    private void SetUp()
    {
        chatScrollbar = chatScrollRect.verticalScrollbar;
        chathandle = chatScrollbar.handleRect;
        content = chatScrollRect.content;
        textViewport = inputField.textViewport;
        isChatting = false;
        SetUpHints();
    }
    private void UpdateTimers()
    {
        for (int i = 0; i < timers.Length; i++)
        {
            if (indexes[i] != -1)
            {
                timers[i]++;
                if (timers[i] >= timeToDisappear)
                {
                    int index = indexes[i];

                    if (index == content.childCount - 1) isTimer = false;
                    if (!isChat)
                    {
                        content.GetChild(index).gameObject.SetActive(false);
                    }
                    content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    
                    indexes[i] = -1;                 
                }
            }
        }
    }
    private void AddNewTimer(int index)
    {
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] == -1)
            {
                indexes[i] = index;
                timers[i] = 0;
                return;
            }
        }

        for (int i = 0; i < indexes.Length; i++)
        {
            if (LastIndex - maxMessageOnScreen == indexes[i])
            {
                if (!isChat)
                {
                    content.GetChild(indexes[i]).gameObject.SetActive(false);
                }
                content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                indexes[i] = index;
                timers[i] = 0;
                return;
            }
        }

    }
    private void ClearIndexes()
    {
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = -1;
        }
    }
    private void SetTimerToDisappear()
    {
        isTimer = true;
        LastIndex = content.childCount - 1;
        AddNewTimer(LastIndex);
        if (content.childCount > maxLog + 1)
        {
            Destroy(content.GetChild(1).gameObject);
            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] != -1) indexes[i]--;
            }
        }
    }
    private void SendMessage()
    {
        string text = inputField.text.Trim();
        SwitchChat();
        if (text.Length > 0)
        {
            if (text[0] == '/' && !IsLocalCommand(text))
            {
                ChatService.ClientSendsMessage(inputField.text);
                SaveToHistory();
            }
            else
            {
                ChatService.ClientSendsMessage(inputField.text);
                SaveToHistory();
            }
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

    public Transform Print(string text)
    {
        Transform message = Instantiate(messagePrefab, content).transform;
        TextMeshProUGUI textMeshProUGUI = message.GetChild(0).GetComponent<TextMeshProUGUI>();
        Image image = message.GetComponent<Image>();
        textMeshProUGUI.color = Color.white; ;
        textMeshProUGUI.text = text;
        chatScrollbar.value = 0;
        SetTimerToDisappear();

        if (isChat)
            image.color = new Color(1, 1, 1, 1f);
        else
            image.color = new Color(1, 1, 1, 0.5f);

        return message;
    }
    public void PrintPlayerMessage(long time,string text, string player)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(time);
        DateTime localTime = date.ToLocalTime().DateTime;
        Print($"[{localTime.ToString("HH:mm:ss")}] <Color=#E68D31>{player}</color>: <noparse>{text}</noparse>");
    }
    public void PrintServerMessage(long time, string text)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(time);
        DateTime localTime = date.ToLocalTime().DateTime;
        Print($"[{localTime.ToString("HH:mm:ss")}] {text}").GetComponent<Image>().sprite = UIConfig.GetBackgroundSprite(Backgrounds.BackgroundType.Iron);
    }  
    private void PrintHint(params CommandBase[] commandBase)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < commandBase.Length; i++)
        {
            sb.Append($"/<Color=#{highlightColor.ToHexString()}>{commandBase[i].commandId}</Color> {commandBase[i].commandFormat}");
            if(i != commandBase.Length - 1) sb.Append("\n");
        }
        if(sb.Length > 0) Print(sb.ToString());
    }
    
    private void SwitchChat()
    {
        currentLastHistory = -1;
        if (isChat) TurnOffChat();
        else TurnOnChat();        
    }
    private void TurnOnChat()
    {
        isChat = true;
        isChatting = true;

        inputField.gameObject.SetActive(true);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
        background.color = Color.white;
        chatScrollbar.value = 0;
        chathandle.gameObject.SetActive(true);
        chatScrollRect.enabled = true;
        for (int i = 1; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] != -1)
            {
                content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
        }
    }
    private void TurnOffChat()
    {
        isChatting = false;
        isChat = false;
        inputField.gameObject.SetActive(false);
        hintParent.gameObject.SetActive(false);

        background.color = new Color(1, 1, 1, 0);
        chatScrollbar.value = 0;
        chathandle.gameObject.SetActive(false);
        chatScrollRect.enabled = false;
        for (int i = 1; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] != -1)
            {
                content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

    public bool IsLocalCommand(string text)
    {
        string[] properties = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0, 1);
        List<CommandBase> hints = new List<CommandBase>();
        
        if (commandList.TryGetValue(properties[0].ToLower(),out var list))
        {
            string[] args = properties.Skip(1).ToArray();
            foreach(var command in list)
            {               
                if (command.Validate(args))
                {
                    if (!command.isServerCommand && !command.isAdminCommand)
                    {
                        var ecb = new EntityCommandBuffer(Allocator.Temp);
                        var output = command.Invoke(args,ref ecb,Entity.Null);
                        ecb.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
                        ecb.Dispose();
                        if (!string.IsNullOrEmpty(output))
                            Print(output);
                        return true;
                    }
                    else
                        return false;
                }
                else 
                    hints.Add(command);
            }
        }

        if (hints.Count == 0)
            Print($"<Color=red>Incorrect command: </Color><noparse>{text}</noparse>");
        PrintHint(hints.ToArray());
        return true;
    }
    public Vector3 GetCaretWorldPosition()
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
    


    #region  Hints
    public void UpdateHints(string newValue)
    {
        if(!ChatService.IsCommand(newValue))
        {
            hintParent.gameObject.SetActive(false);
            return;
        }
        string[] command = newValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandID = command[0].Substring(1).ToLower();

        if(commandList.TryGetValue(commandID,out var variantlist))
        {
            
        }
        else
        {
            var hints = commandList.Where(x => x.Key.StartsWith(commandID)).SelectMany(x => x.Value).ToList();
            UpdateHintList(hints);
        }

        if(hintList.Count > 0)
        {
            hintParent.gameObject.SetActive(true);
            Debug.Log("sok " + textViewport.rect.height);
            var position = GetCaretWorldPosition() + new Vector3(0,textViewport.rect.height - 12f);
            hintParent.position = position;
        }
        else
            hintParent.gameObject.SetActive(false);
    }
    public void SetUpHints()
    {
        var sprite = UIConfig.GetBackgroundSprite(Backgrounds.BackgroundType.Wood);
        Instantiate(hintPrefab,hintParent).GetComponent<Image>().sprite = UIConfig.GetBackgroundSprite(Backgrounds.BackgroundType.Iron);
        for(int i = 0; i < displayedHintsCount - 1; i++)
            Instantiate(hintPrefab,hintParent).GetComponent<Image>().sprite = sprite;
    }
    public void UpdateHintList(List<CommandBase> hints)
    {
        hintList = hints;
        selectedHint = 0;   
        if(hints.Count == 0) return;  
        for(int i = 0; i < hintParent.childCount; i++)
        {
            var child = hintParent.GetChild(i);
            if(i < hints.Count)
            {
                var command = hints[i];
                child.gameObject.SetActive(true);
                child.GetComponentInChildren<TextMeshProUGUI>().text = $"/<Color=#{highlightColor.ToHexString()}>{command.commandId}</Color> {command.commandFormat}";
            }
            else
                child.gameObject.SetActive(false);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(hintParent);
    }
    #endregion
}
 