using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Image background;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private RectTransform hints;    

    [Header("Prefabs")]
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject hintPrefab;

    //chat
    private Scrollbar chatScrollbar;
    private RectTransform chathandle;
    private Transform content;
    //

    //History
    private List<string> history = new List<string>();
    private int LastHistory;
    private int currentLastHistory = -1;

    private const int maxHistory = 10;
    private const int maxLog = 30;
    private const int maxMessageOnScreen = 8;
    private const int timeToDisappear = 10;
    public static readonly Color highlightColor = new Color(218f / 255f, 165f / 255f, 32f / 255f);

    private static Color invisible = new Color(1, 1, 1, 0);

    private bool isTimer = false;
    private  int LastIndex = 0;
    public int[] indexes = new int[maxMessageOnScreen];
    public int[] timers = new int[maxMessageOnScreen];
    //

    private bool isChat = false;
    private List<CommandBase> commandList = new List<CommandBase>();

    public static ChatManager instance { private set; get; }
    public bool isChatting { private set; get; }
    
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        SetUp();  
    }
    private void Start()
    {
        ClearIndexes();
        commandList = DebugController.CommandList;
        hints.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);
        
    }
    private void OnEnable()
    {
        inputField.onValueChanged.AddListener(UpdateHints);
    }
    private void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(UpdateHints);
    }

    float timer = 0;
    private void Update()
    {     
        if(isTimer)
        {
            timer += Time.deltaTime;
            if(timer >= 1)
            {
                timer = 0;
                UpdateTimers();
            }
        }

        if (Input.GetKeyDown(KeyCode.T) && !inputField.isFocused && !WindowsManager.instance.HasOpenWidnows())
        {
            SwitchChat();
        }

        if(isChat)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessage();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SwitchChat();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentLastHistory = math.clamp(++currentLastHistory, 0, history.Count - 1);
                int index = LastHistory - currentLastHistory;


                if (index < 0) index = history.Count + index;
                if(index < history.Count) inputField.text = history[index];
            }
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentLastHistory = math.clamp(--currentLastHistory, 0, history.Count - 1);
                int index = LastHistory - currentLastHistory;

                if (index < 0) index = history.Count + index;
                if (index < history.Count) inputField.text = history[index];
            }
        }
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
            if (text[0] == '/')
            {
                if(CheckCommands(text))
                {
                    ChatService.ClientSendsMessage(inputField.text);
                    SaveToHistory();
                }
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
        Print($"[{localTime.ToString("HH:mm:ss")}] {text}").GetComponent<Image>().sprite = UIAssetsManager.instance.ironBackgroundUI;
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
    private void SetUp()
    {
        chatScrollbar = chatScrollRect.verticalScrollbar;
        chathandle = chatScrollbar.handleRect;
        content = chatScrollRect.content;
        isChatting = false;
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
        hints.gameObject.SetActive(false);

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

    public bool CheckCommands(string command)
    {
        string[] properties = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0, 1);
        List<CommandBase> hints = new List<CommandBase>();
        foreach (var item in commandList)
        {
            CommandBase commandBase = item as CommandBase;
            if (string.Compare(commandBase.commandId, properties[0], true) == 0)
            {
                string[] args = properties.Skip(1).ToArray();
                if (item.Validate(args))
                {
                    if (!item.isServerCommand && !item.isAdminCommand)
                    {
                        var ecb = new EntityCommandBuffer(Allocator.Temp);
                        var output = item.Invoke(args,ref ecb,Entity.Null);
                        ecb.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
                        ecb.Dispose();
                        if (!string.IsNullOrEmpty(output))
                            Print(output);
                        return false;
                    }
                    else
                        return true;
                }
                else hints.Add(commandBase);
            }
        }

        if (hints.Count == 0)
            Print($"<Color=red>Incorrect command: </Color><noparse>{command}</noparse>");
        PrintHint(hints.ToArray());
        return false;
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
    public void UpdateHints(string newValue)
    {
        Debug.Log("update!!");
        var position = GetCaretWorldPosition() + new Vector3(0,50f);
        hints.position = position;
    }
}
 