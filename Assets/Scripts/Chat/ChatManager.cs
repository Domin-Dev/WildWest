using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using System;
using Unity.Entities;
using Unity.NetCode;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Image chatBackground;
    [SerializeField] private ScrollRect chatScrollRect;

    [SerializeField] private GameObject messagePrefab;


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
    //

    //Massages
    private const int maxLog = 20;
    private const int maxMessageOnScreen = 8;
    private const int timeToDisappear = 10;
    private bool isTimer = false;
    private  int LastIndex = 0;
    private int[] indexes = new int[maxMessageOnScreen];
    private int[] timers = new int[maxMessageOnScreen];
    //

    private bool isChat = false;

    private List<object> commandList;

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
        chatInputField.onSelect.AddListener((string k) => { isChatting = true; });
        chatInputField.onDeselect.AddListener((string k) => { isChatting = false; SwitchChat();});
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

        if (Input.GetKeyDown(KeyCode.T) && !chatInputField.isFocused)
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
                if(index < history.Count) chatInputField.text = history[index];
            }
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentLastHistory = math.clamp(--currentLastHistory, 0, history.Count - 1);
                int index = LastHistory - currentLastHistory;

                if (index < 0) index = history.Count + index;
                if (index < history.Count) chatInputField.text = history[index];
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
                        content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    }
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
                                            content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                }

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
        if (content.childCount > maxLog)
        {
            Destroy(content.GetChild(0).gameObject);
            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] != -1) indexes[i]--;
            }
        }
        LastIndex = content.childCount - 1;
        AddNewTimer(LastIndex);
    }
    private void SendMessage()
    {
        if (chatInputField.text.Length > 0)
        {
            SendRPC(chatInputField.text);
            SaveToHistory();
            CheckCommands();
        }
        SwitchChat();
    }
    private void SaveToHistory()
    {
        if (history.Count < maxHistory)
        {
            history.Add(chatInputField.text);
            LastHistory = history.Count - 1;
        }
        else
        {
            LastHistory++;
            if (LastHistory >= maxHistory)
            {
                LastHistory = 0;
            }

            history[LastHistory] = chatInputField.text;
        }
    }
    private void SendRPC(string value)
    {
        Debug.Log("new rpc");
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        Entity messageEntity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(messageEntity, new NewMessageRPC() { message = value.Substring(0, Math.Min(511, value.Length)) });
        entityCommandBuffer.AddComponent(messageEntity, new SendRpcCommandRequest());

        entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }
    public void Print(string text)
    {
        Transform message = Instantiate(messagePrefab, content).transform;
        message.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        chatScrollbar.value = 0;
        SetTimerToDisappear();
    }
    public void PrintPlayerMessage(long time,string text, string player)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(time);
        DateTime localTime = date.ToLocalTime().DateTime;

        Transform message = Instantiate(messagePrefab, content).transform;
        message.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"[{localTime.ToString("HH:mm:ss")}] <Color=#E68D31>{player}</color>: {text}";
        chatScrollbar.value = 0;
        SetTimerToDisappear();
    }
    private void CheckCommands()
    {
        string command = chatInputField.text;

        for (int i = 0; i < command.Length; i++)
        {
            char c = command[i];
            if (c == '/')
            {
                break;
            }
            else if (command[i] != ' ')
            {
                return;
            }
        }

        string[] properties = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        properties[0] = properties[0].Remove(0,1);
        List<CommandBase> hints = new List<CommandBase>();

        foreach (var item in commandList)
        {
            CommandBase commandBase = item as CommandBase;
            if (string.Compare(commandBase.commandId, properties[0], true) == 0)
            {
                if (item is DebugCommand)
                {
                    (item as DebugCommand).Invoke();
                    return;
                }
                else if (item is DebugCommand<int>)
                {
                    int arg;
                    if (properties.Length > 1 && int.TryParse(properties[1], out arg))
                    {      
                        (item as DebugCommand<int>).Invoke(arg);
                        return;
                    }
                    else hints.Add(commandBase);
                }
                else if (item is DebugCommand<int,int>)
                {
                    int arg1,arg2;
                    if (properties.Length > 2 && int.TryParse(properties[1], out arg1) && int.TryParse(properties[2], out arg2))
                    {
                        (item as DebugCommand<int, int>).Invoke(arg1, arg2);
                        return;
                    }
                    else hints.Add(commandBase);
                }
                else if (item is DebugCommand<int, int, int>)
                {
                    int arg1, arg2, arg3;
                    if (properties.Length > 2 && int.TryParse(properties[1], out arg1) && int.TryParse(properties[2], out arg2) && int.TryParse(properties[3], out arg3))
                    {
                        (item as DebugCommand<int, int,int>).Invoke(arg1, arg2,arg3);
                        return;
                    }
                    else hints.Add(commandBase);
                }
            }
        }

        foreach (CommandBase item in hints)
        {
            PrintHint(item);
        }
    }
    private void PrintHint(CommandBase commandBase)
    {
        Print($"/{commandBase.commandId} {commandBase.commandFormat}");
    }
    private void SetUp()
    {
        chatScrollbar = chatScrollRect.verticalScrollbar;
        chathandle = chatScrollbar.handleRect;
        content = chatScrollRect.content;
        commandList = this.AddComponent<DebugController>().GetCommandList();
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
        chatInputField.gameObject.SetActive(true);
        chatInputField.text = string.Empty;
        chatInputField.ActivateInputField();
        chatBackground.color = Color.white;
        chatScrollbar.value = 0;
        chathandle.gameObject.SetActive(true);
        chatScrollRect.enabled = true;
        for (int i = 0; i < content.childCount; i++)
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
        chatInputField.gameObject.SetActive(false);
        chatBackground.color = new Color(1, 1, 1, 0);
        chatScrollbar.value = 0;
        chathandle.gameObject.SetActive(false);
        chatScrollRect.enabled = false;
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] != -1)
            {
                content.GetChild(indexes[i]).gameObject.SetActive(true);
                content.GetChild(indexes[i]).GetComponent<Image>().color = new Color(1, 1, 1, 0);
            }
        }
    }

}
