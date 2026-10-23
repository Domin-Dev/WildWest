using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatConfig", menuName = "GameAsset/ConfigFiles/ChatConfig")]
public class ChatConfig : Config<ChatConfig>
{
    [SerializeField] private int maxMessageOnScreen = 8;
    [Suffix("Seconds")]
    [SerializeField] private int timeToDisappear = 10;
    [SerializeField] private int maxLog = 30;
    [Min(1)] 
    [SerializeField] private int maxHintsOnScreen = 5;
    [SerializeField] private Color highlightColor = new Color(218f / 255f, 165f / 255f, 32f / 255f);
    [SerializeField] private Color warningColor = Color.red;

    public int MaxMessageOnScreen => maxMessageOnScreen;
    public int TimeToDisappear => timeToDisappear;
    public int MaxLog => maxLog;
    public int MaxHintsOnScreen => maxHintsOnScreen;
    public Color HighlightColor => highlightColor;
    public Color WarningColor => warningColor;
}
