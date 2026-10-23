using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChatCore<T> : Singleton<T> where T : ChatCore<T>
{
    #region Fields
    public bool ChatIsOpen { private set; get; }
    protected virtual bool canOpen => !WindowsManager.instance.HasOpenWidnows();

    [Header("Core")]
    [SerializeField] protected TMP_InputField inputField;
    [SerializeField] private Image background;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private GameObject messagePrefab;

    [Header("Config")]
    [SerializeField] private int maxMessageOnScreen = 8;
    [SerializeField] private int timeToDisappear = 10;
    [SerializeField] private int maxLog = 30;
    [Header("Input")]
    [SerializeField] private InputActionReference openChatAction;
    [SerializeField] private InputActionReference closeChatAction;
    [SerializeField] private InputActionReference sendMessageAction;

    private Scrollbar chatScrollbar;
    private RectTransform chathandle;
    private RectTransform content;
    private int lastIndex => content.childCount - 1;
    private List<(float timer,int index)> messagesOnScreenList;
    private TimersForWithUpdateInterval<int> messagesOnScreen;
    #endregion
    #region UnityFuncs
    protected override void Awake()
    {
        base.Awake();
        SetUp();  
    }
    protected virtual void OnEnable()
    {
        openChatAction.action.performed += OpenChat;
        closeChatAction.action.performed += CloseChat;
        sendMessageAction.action.performed += SendMessage;
    }
    protected virtual void OnDisable()
    {
        openChatAction.action.performed -= OpenChat;
        closeChatAction.action.performed -= CloseChat;
        sendMessageAction.action.performed -= SendMessage;
    }
    private void OnDestroy()
    {
        messagesOnScreen.Cancel();
    }
    #endregion
    #region Funcs
    public Transform Print(string text)
    {
        Transform message = Instantiate(messagePrefab, content).transform;
        TextMeshProUGUI textMeshProUGUI = message.GetChild(0).GetComponent<TextMeshProUGUI>();
        Image image = message.GetComponent<Image>();
        textMeshProUGUI.color = Color.white; ;
        textMeshProUGUI.text = text;
        chatScrollbar.value = 0;
        SetTimerToDisappear();

        if (ChatIsOpen)
            image.color = new Color(1, 1, 1, 1f);
        else
            image.color = new Color(1, 1, 1, 0.5f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(message.GetComponent<RectTransform>());
        return message;
    }
    public void Clear()
    {
        messagesOnScreenList.Clear();
        for (int i = 1; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);
        LayoutRebuilder.MarkLayoutForRebuild(content);
    }
    protected virtual void SetUp()
    {
        chatScrollbar = chatScrollRect.verticalScrollbar;
        chathandle = chatScrollbar.handleRect;
        content = chatScrollRect.content;
        ChatIsOpen = false;
        inputField.gameObject.SetActive(false);  

        messagesOnScreenList = new List<(float,int)>();
        messagesOnScreen = Timer.Create(messagesOnScreenList,(index) => 
        {
            if (!ChatIsOpen)
                content.GetChild(index).gameObject.SetActive(false);  
            content.GetChild(index).GetComponent<Image>().color = new Color(1, 1, 1, 1);            
            return false;
        },1f,false);
    }
    protected virtual void SwitchChat()
    {
        if (ChatIsOpen) TurnOffChat();
        else TurnOnChat();        
    }
    protected virtual void TurnOnChat()
    {
        ChatIsOpen = true;
        inputField.gameObject.SetActive(true);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
        background.color = Color.white;
        chatScrollbar.value = 0;
        chathandle.gameObject.SetActive(true);
        chatScrollRect.enabled = true;

        for (int i = 1; i < content.childCount; i++)
            content.GetChild(i).gameObject.SetActive(true);
        
        for (int i = 0; i < messagesOnScreenList.Count; i++)
                content.GetChild(messagesOnScreenList[i].index).GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }
    protected virtual void TurnOffChat()
    {
        ChatIsOpen = false;
        inputField.gameObject.SetActive(false);
        background.color = new Color(1, 1, 1, 0);
        chatScrollbar.value = 0;
        chathandle.gameObject.SetActive(false);
        chatScrollRect.enabled = false;

        for (int i = 1; i < content.childCount; i++)
            content.GetChild(i).gameObject.SetActive(false);
    }
    protected virtual void ProcessMessage(string message)
    {
        Print(message);
    }
    private void AddNewTimer(int index)
    {
        if(messagesOnScreen.Count >= maxMessageOnScreen)
            messagesOnScreen.RemoveValue(lastIndex - maxMessageOnScreen);
        messagesOnScreen.AddValue(timeToDisappear,index);
    }
    private void SetTimerToDisappear()
    {
        AddNewTimer(lastIndex);
        if (content.childCount > maxLog + 1)
        {
            Destroy(content.GetChild(1).gameObject);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            for (int i = 0; i < messagesOnScreenList.Count; i++)
            {
                var value = messagesOnScreenList[i];
                messagesOnScreenList[i] = (value.timer,value.index-1);
            }
        }
    }
    private void SendMessage()
    {
        string text = inputField.text.Trim();
        SwitchChat();
        if (text.Length > 0)
            ProcessMessage(text);
    }
    #endregion   
    #region InputActions
    private void OpenChat(InputAction.CallbackContext context)
    {
        if(!inputField.isFocused && canOpen)
            SwitchChat();
    }
    private void CloseChat(InputAction.CallbackContext context)
    {
        if(ChatIsOpen)
            SwitchChat();
    }
    private void SendMessage(InputAction.CallbackContext context)
    {
        if(ChatIsOpen)
            SendMessage();
    }
    #endregion
}