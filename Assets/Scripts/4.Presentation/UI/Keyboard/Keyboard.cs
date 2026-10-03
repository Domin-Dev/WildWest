using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    [SerializeField] private Transform keysParent;
    [SerializeField] private GameObject keyPrefab;
    [Space]
    [SerializeField] private Button capsLock;
    [SerializeField] private Button space;
    [SerializeField] private Button backspace;
    [SerializeField] private KeyboardInput input;
    [SerializeField] private Button back;
    [SerializeField] private Button save;
    [Space]
    [SerializeField] private bool numbersEnabled = true;
    [SerializeField] private bool lettersEnabled = true;
    [SerializeField] private bool specialCharsEnabled = true;
 

    [SerializeField] private bool autoBuild = true;

    public Action<string> onSaveChanges;
    private bool capsLockOn = false;
    private int columns;
    private int counter;
    private Button[,] buttons;


    private const int funKeySize = 2;
    private static string[] numbers = new string[]
    {
        "1","2","3","4","5","6","7","8","9","0",
    };
    private static string[] keys = new string[]
    {
        "q","w","e","r","t","y","u","i","o","p",
        "a","s","d","f","g","h","j","k","l",";",
        "z","x","c","v","b","n","m",",",".","/"
    };



    private void Start()
    {
        if(autoBuild) Build(numbersEnabled, lettersEnabled, specialCharsEnabled);   
    }


    private void Save()
    {
        onSaveChanges?.Invoke(input.text);
    }
    private IEnumerator SetBackNextFrame()
    {
        yield return null; 
        EventSystem.current.SetSelectedGameObject(back.gameObject);
    }
    public void Build(bool numbersEnabled = true, bool lettersEnabled = true, bool specialCharsEnabled = true)
    {
        this.numbersEnabled = numbersEnabled;
        this.lettersEnabled = lettersEnabled;
        this.specialCharsEnabled = specialCharsEnabled;
        this.columns = keysParent.GetComponent<GridLayoutGroup>().constraintCount;
        this.counter = 0;
        buttons = new Button[columns,(int)Math.Ceiling(((float)numbers.Length + keys.Length)/(float)columns)];
        
        input.caretPosition = input.text.Length;
        input.ActivateInputField();


        if (numbersEnabled)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                CreateNewKey(numbers[i]);
            }
        }
        for (int i = 0; i < keys.Length; i++)
        {
            if (char.IsLetter(keys[i][0]))
            {
                CreateNewKey(keys[i], lettersEnabled);
            }
            else
            {
                CreateNewKey(keys[i], specialCharsEnabled);     
            }
        }
        

        capsLock.onClick.AddListener(CapsLock);
        space.onClick.AddListener(Space);
        backspace.onClick.AddListener(Backspace);
        save.onClick.AddListener(Save);

        backspace.GetComponent<ButtonHold>().action += Backspace;
        SetUpNavigation();
        StartCoroutine(SetBackNextFrame());
    }
    private void SetUpNavigation()
    {
        int cols = buttons.GetLength(0);
        int rows = buttons.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (buttons[c, r] == null) continue;
                var nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;

                if (r > 0) nav.selectOnUp = buttons[c,r - 1];
                if (r < rows - 1) nav.selectOnDown = buttons[c,r + 1];
                
                if (c > 0) nav.selectOnLeft = buttons[c - 1,r];
                else nav.selectOnLeft = buttons[cols - 1, r];

                if (c < cols - 1) nav.selectOnRight = buttons[c + 1, r];
                else nav.selectOnRight = buttons[0, r];

                if (nav.selectOnDown == null)
                {
                    if (c < funKeySize)
                        nav.selectOnDown = capsLock;
                    else if (c > cols - 1 - funKeySize)
                        nav.selectOnDown = backspace;
                    else
                        nav.selectOnDown = space;
                }

                buttons[c,r].navigation = nav;
            }
        }
    }
    private void CreateNewKey(string key, bool enabled = true)
    {
        GameObject btnObj = Instantiate(keyPrefab, keysParent);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = key;
        Button btn = btnObj.GetComponent<Button>();
        ButtonHold buttonHold = btnObj.GetComponent<ButtonHold>();

        btn.onClick.AddListener(() => { AddChar(key); });
        buttonHold.action += () => { AddChar(key); };

        btn.interactable = enabled;
        buttonHold.interactable = enabled;

        if (!enabled) btn.GetComponent<Image>().color = new Color(1, 1, 1, 0.6f);
        buttons[counter%columns,counter/columns] = btn;
        counter++;
    }
    private void AddChar(string value)
    {
        if (capsLockOn)
            value = value.ToUpper();
        input.text += value;
        input.caretPosition = input.text.Length;
    }
    private void CapsLock()
    {
        capsLockOn = !capsLockOn;      
        for (int i = 0; i < keysParent.childCount; i++)
        {
            TextMeshProUGUI tmp = keysParent.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            string text = keysParent.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text;
            if (capsLockOn)
                text = text.ToUpper();
            else
                text = text.ToLower();
            tmp.text = text;
        }      
    }
    private void Space()
    {
        string x = input.text;
        if (x.Length > 0 && x[x.Length - 1] != ' ') 
            input.text += ' ';
        input.caretPosition = input.text.Length;
    }
    private void Backspace()
    {
        string x = input.text;
        if (x.Length > 0)
            input.text = x.Remove(x.Length - 1, 1);
        input.caretPosition = input.text.Length;
    }
}
