
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button errorButton;

    private void Awake()
    {
        errorButton.onClick.AddListener(() => { SceneManager.LoadScene(0);});
    }

    private void Start()
    {
        errorText.text = GameInfo.instance.errorMessage;
    }
}
