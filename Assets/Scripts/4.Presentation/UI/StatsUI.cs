


using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private Image image;

    public void SetUp(string text,Color color)
    {
        textUI.text = text;
        image.color = color;
    }
}
