
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    [SerializeField] private Tooltip tooltip;
    private static TooltipSystem current;

    private static Timer timer;

    private void Awake()
    {
        if(current == null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    public static void Show(string content,string header = "")
    {
        timer = Timer.Create(0.45f,() => { current.tooltip.SetText(content, header); return false;});
    }
    public static void ShowInstant(string content, string header = "")
    {
       current.tooltip.SetText(content, header);
    }

    public static void Hide()
    {
        if(timer != null) timer.Cancel();
        current.tooltip.Hide();
    }


}
