using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Row : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI playerName;
    [SerializeField] public Image playerIcon;
    [SerializeField] public TextMeshProUGUI pingText;
}

public class AdminRow : Row
{
    [SerializeField] public Button manageButton;
    [SerializeField] public Button banButton;
}